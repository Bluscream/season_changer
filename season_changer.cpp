#ifdef _WIN32
#  define WINVER 0x0500
#  define _WIN32_WINNT 0x0500
#  include <windows.h>
#endif

#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <stdarg.h>
#include <string.h>
#include <string>
#include <fstream>
#include <streambuf>

#include "scssdk_telemetry.h"
#include "eurotrucks2/scssdk_eut2.h"
#include "eurotrucks2/scssdk_telemetry_eut2.h"
#include "amtrucks/scssdk_ats.h"
#include "amtrucks/scssdk_telemetry_ats.h"

#define UNUSED(x)

FILE *log_file = NULL;

bool output_paused = true;

bool print_header = true;

scs_timestamp_t last_timestamp = static_cast<scs_timestamp_t>(-1);

struct telemetry_state_t
{
	scs_timestamp_t timestamp;
	scs_timestamp_t raw_rendering_timestamp;
	scs_timestamp_t raw_simulation_timestamp;
	scs_timestamp_t raw_paused_simulation_timestamp;

	bool	orientation_available;
	float	heading;
	float	pitch;
	float	roll;

	float	speed;
	float	rpm;
	int	gear;

} telemetry;

scs_log_t game_log = NULL;

bool init_log(void)
{
	if (log_file) {
		return true;
	}
	log_file = fopen("plugins/season_changer/season_changer.log", "wt");
	if (!log_file) {
		return false;
	}
	fprintf(log_file, "Log opened\n");
	return true;
}

void finish_log(void)
{
	if (!log_file) {
		return;
	}
	fprintf(log_file, "Log ended\n");
	fclose(log_file);
	log_file = NULL;
}

void log_print(const char *const text, ...)
{
	if (!log_file) {
		return;
	}
	va_list args;
	va_start(args, text);
	vfprintf(log_file, text, args);
	va_end(args);
}

void log_line(const char *const text, ...)
{
	if (!log_file) {
		return;
	}
	va_list args;
	va_start(args, text);
	vfprintf(log_file, text, args);
	fprintf(log_file, "\n");
	va_end(args);
}

SCSAPI_VOID telemetry_frame_start(const scs_event_t UNUSED(event), const void *const event_info, const scs_context_t UNUSED(context)) {
}

SCSAPI_VOID telemetry_frame_end(const scs_event_t UNUSED(event), const void *const UNUSED(event_info), const scs_context_t UNUSED(context)) {
}

SCSAPI_VOID telemetry_pause(const scs_event_t event, const void *const UNUSED(event_info), const scs_context_t UNUSED(context)) {
}

SCSAPI_VOID telemetry_configuration(const scs_event_t event, const void *const event_info, const scs_context_t UNUSED(context)) {
}

SCSAPI_VOID telemetry_store_orientation(const scs_string_t name, const scs_u32_t index, const scs_value_t *const value, const scs_context_t context) {
}

SCSAPI_VOID telemetry_store_float(const scs_string_t name, const scs_u32_t index, const scs_value_t *const value, const scs_context_t context) {
}

SCSAPI_VOID telemetry_store_s32(const scs_string_t name, const scs_u32_t index, const scs_value_t *const value, const scs_context_t context) {
}

SCSAPI_RESULT scs_telemetry_init(const scs_u32_t version, const scs_telemetry_init_params_t *const params)
{
	if (version != SCS_TELEMETRY_VERSION_1_00) {
		return SCS_RESULT_unsupported;
	}

	const scs_telemetry_init_params_v100_t *const version_params = static_cast<const scs_telemetry_init_params_v100_t *>(params);
	if (! init_log()) {
		version_params->common.log(SCS_LOG_TYPE_error, "Unable to initialize the log file");
		return SCS_RESULT_generic_error;
	}

	log_line("Game '%s' %u.%u", version_params->common.game_id, SCS_GET_MAJOR_VERSION(version_params->common.game_version), SCS_GET_MINOR_VERSION(version_params->common.game_version));

	if (strcmp(version_params->common.game_id, SCS_GAME_ID_EUT2) != 0) {
		log_line("UNSUPPORTED GAME: SHUTTING DOWN!");
		return SCS_RESULT_generic_error;
	}

	game_log = version_params->common.log;
	game_log(SCS_LOG_TYPE_message, "Initializing season_changer log example");

	memset(&telemetry, 0, sizeof(telemetry));
	print_header = true;
	last_timestamp = static_cast<scs_timestamp_t>(-1);

	output_paused = true;

	std::string template_manifest_path = "season_changer/template/manifest.sii";
	std::ifstream t(template_manifest_path);
	std::string template_manifest;
	t.seekg(0, std::ios::end);
	/*
	template_manifest.reserve(t.tellg());
	t.seekg(0, std::ios::beg);
	template_manifest.assign((std::istreambuf_iterator<char>(t)),
		std::istreambuf_iterator<char>());
	
	log_line("Loaded \"%s\":", template_manifest_path);
	log_line(template_manifest.c_str());

	std::string template_env_data_path = "season_changer/template/def/env_data.sii";
	std::ifstream t2(template_env_data_path);
	std::string template_env_data;
	t.seekg(0, std::ios::end);
	template_env_data.reserve(t.tellg());
	t.seekg(0, std::ios::beg);
	template_env_data.assign((std::istreambuf_iterator<char>(t)),
		std::istreambuf_iterator<char>());

	log_line("Loaded \"%s\":", template_env_data_path);
	log_line(template_env_data.c_str());*/
	
	return SCS_RESULT_ok;
}

SCSAPI_VOID scs_telemetry_shutdown(void)
{
	game_log = NULL;
	finish_log();
}

#ifdef _WIN32
BOOL APIENTRY DllMain(
	HMODULE module,
	DWORD  reason_for_call,
	LPVOID reseved
)
{
	if (reason_for_call == DLL_PROCESS_DETACH) {
		finish_log();
	}
	return TRUE;
}
#endif

#ifdef __linux__
void __attribute__ ((destructor)) unload(void)
{
	finish_log();
}
#endif
