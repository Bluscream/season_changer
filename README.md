# ETS 2 Auto Season/DayOfYear/Summertime Tool

## Description

This tool allows syncing the day of year and summertime ingame with real-world values.
It also allows you to specify seasons which can by linked for you at certain 

## Installation

1. Download the latest release from the [releases](https://github.com/Bluscream/season_changer/releases) page.
2. Extract `Season.Changer.zip` to your SCS game folder (Example: `C:\Program Files (x86)\Steam\Steamapps\common\Euro Truck Simulator 2\`).
3. Edit the configuration file located at `<Euro Truck Simulator 2>\bin\win_x64\plugins\season_changer\Seasons.ini` to your needs. (You can add/remove as many seasons as you like as long as the days don't cross)
4. A) Start `<Euro Truck Simulator 2>\bin\win_x64\plugins\season_changer\Season Changer.exe` manually for it do it's work
4. B) Download and install [scs-autostart](https://github.com/Bluscream/scs-autostart#installation) and add `plugins/season_changer/Season Changer.exe` to the `after_sdk_warning=` line.

## Example Configuration

```ini
[Spring]
File = SEASON - Spring.scs
Days = 60-149

[Summer]
File = SEASON - Summer.scs
Days = 150-209

[Early Autumn]
File = SEASON - Early Autumn.scs
Days = 210-239

[Late Autumn]
File = SEASON - Late Autumn.scs
Days = 240-329

[Winter]
File = SEASON - Winter.scs
Days = 330-59
```

## Known Issues

- Currently only works with ETS 2
- Currently only works on Windows

## Notes

Original / Inspiration: https://gist.github.com/Bluscream/b7fa18cb6b935ecadc2a19bbad022ec5
