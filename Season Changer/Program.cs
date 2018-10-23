using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Season_Changer
{
    static class Program
    {
        [STAThread]
        static void Main() {
            var parser = new FileIniDataParser();
            var seasonspath = "Seasons.ini";
            var seasons_ = parser.ReadFile(seasonspath);
            List<Season> seasons = new List<Season>();
            foreach (var item in seasons_.Sections)
            {
                var days = item.Keys["Days"].Split('-');
                var curseason = new Season(item.SectionName, item.Keys["File"], days[0], days[1], item.Keys["Mod"], item.Keys["Extras"]);
                seasons.Add(curseason);
            }
            DateTime now = DateTime.Now;
            var dayofyear = now.DayOfYear;
            Season season = null;
            foreach (var season_ in seasons)
            {
                if (dayofyear > season_.FromDay && dayofyear < season_.ToDay)
                    season = season_;

            }
            if(season is null) { 
                MessageBox.Show($"No valid season found for day {dayofyear}! Make sure your {seasonspath} is correct!");
                throw new Exception($"No valid season found!");
            }
            var summertime = TimeZoneInfo.Local.IsDaylightSavingTime(now);
            var description = File.ReadAllText("template/description.txt");
            description = description.Replace("{timenow}", now.ToString());
            description = description.Replace("{dayofyear}", dayofyear.ToString());
            description = description.Replace("{summertime}", summertime.ToYesNoString());
            description = description.Replace("{season_name}", season.Name);

            var env_data = File.ReadAllText("template/def/env_data.sii");
            env_data = env_data.Replace("{dayofyear}", dayofyear.ToString());
            env_data = env_data.Replace("{summertime}", Convert.ToInt32(summertime).ToString());

            var modpath = "../../../../mod/";
            Directory.CreateDirectory(modpath);
            File.WriteAllText($"{modpath}Data/description.txt", description);
            Directory.CreateDirectory(modpath + "/def");
            File.WriteAllText($"{modpath}Data/def/env_data.sii", env_data);
            
            var currentfilepath = $"{modpath}SEASON - CURRENT.scs";
            currentfilepath = @"\\?\" + Path.GetFullPath(currentfilepath);
            //var currentfile = new FileInfo(currentfilepath);

            //var path = NativeMethods.GetFinalPathName(currentfilepath);
            var sourcefilepath = $"{modpath}{season.FileName}";
            sourcefilepath = @"\\?\" + Path.GetFullPath(sourcefilepath);
            //var sourcefile = new FileInfo(sourcefilepath);

            //var success = NativeMethods.CreateHardLink(currentfilepath, sourcefilepath, IntPtr.Zero);
            NativeMethods.CreateSymbolicLink(currentfilepath, sourcefilepath, NativeMethods.SymbolicLink.File);
        }
        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        /*static private int getSeason(DateTime date, bool ofSouthernHemisphere = false)
        {
            var hemisphereConst = (ofSouthernHemisphere ? 2 : 0);
            Func<int, int> getReturn = (northern) => {
                return (northern + hemisphereConst) % 4;
            };
            var value = (float)date.Month + date.Day / 100f;
            if (value < 3.21 || value >= 12.22) return getReturn(3); // Winter
            if (value < 6.21) return getReturn(0); // Spring
            if (value < 9.23) return getReturn(1); // Summer
            return getReturn(2); // Autumn
        }*/
    }
    public class Season {
        public string Name { get; set; }
        public string FileName { get; set; }
        public int FromDay { get; set; }
        public int ToDay { get; set; }
        public string OriginalModName { get; set; }
        public string[] Extras { get; set; }
        public Season() { }
        public Season(string name, string filename, string fromday, string today, string originalmodname = null, string extras = null)
        {
            Name = name;
            FileName = filename;
            if (!string.IsNullOrEmpty(originalmodname)) OriginalModName = originalmodname;
            FromDay = Convert.ToInt32(fromday);
            ToDay = Convert.ToInt32(today);
            if(!string.IsNullOrEmpty(extras)) Extras = extras.Split(',');
        }
    }
    public static class BooleanExtensions {
        public static string ToYesNoString(this bool value)
        {
            return value ? "Yes" : "No";
        }
    }
    public static class NativeMethods
    {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private const uint FILE_READ_EA = 0x0008;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
        public enum SymbolicLink {
            File = 0,
            Directory = 1
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CreateHardLink(string lpFileName,string lpExistingFileName,IntPtr lpSecurityAttributes);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] System.Text.StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
                [MarshalAs(UnmanagedType.LPTStr)] string filename,
                [MarshalAs(UnmanagedType.U4)] uint access,
                [MarshalAs(UnmanagedType.U4)] FileShare share,
                IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
                [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
                IntPtr templateFile);

        public static string GetFinalPathName(string path)
        {
            var h = CreateFile(path,
                FILE_READ_EA,
                FileShare.ReadWrite | FileShare.Delete,
                IntPtr.Zero,
                FileMode.Open,
                FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);
            if (h == INVALID_HANDLE_VALUE)
                throw new Win32Exception();

            try
            {
                var sb = new System.Text.StringBuilder(1024);
                var res = GetFinalPathNameByHandle(h, sb, 1024, 0);
                if (res == 0)
                    throw new Win32Exception();

                return sb.ToString();
            }
            finally
            {
                CloseHandle(h);
            }
        }
    }
}
