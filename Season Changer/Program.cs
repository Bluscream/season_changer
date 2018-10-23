using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;

namespace Season_Changer
{
    /*static class Season
    {
         
    }*/
    static class Program
    {
        enum Season
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }
        [STAThread]
        static void Main()
        {
            DateTime now = DateTime.Now;
            var dayofyear = now.DayOfYear;
            var summertime = TimeZoneInfo.Local.IsDaylightSavingTime(now);
            var season = getSeason(now);
            var season_str = season.ToString();
            var manifest = File.ReadAllText("template/manifest.sii");
            manifest = manifest.Replace("{season_name}", season_str);
            var env_data = File.ReadAllText("template/def/env_data.sii");
            env_data = env_data.Replace("{dayofyear}", dayofyear.ToString());
            env_data = env_data.Replace("{summertime}", Convert.ToInt32(summertime).ToString());
            var modpath = "../../../../mod/";
            Directory.CreateDirectory(modpath);
            File.WriteAllText($"{modpath}Data/manifest.sii", manifest);
            Directory.CreateDirectory(modpath + "/def");
            File.WriteAllText($"{modpath}Data/def/env_data.sii", env_data);
            var parser = new FileIniDataParser();
            var cfgpath = "Season Changer.ini";
            if (!File.Exists(cfgpath))
            {
                var newdata = new IniData();
                foreach (var item in (Season[])Enum.GetValues(typeof(Season)))
                {
                    var itemstr = item.ToString();
                    newdata["Seasons"][itemstr] = $"SEASON - {itemstr}.scs";
                }
                newdata["Replace"]["CurrentSeason"] = "SEASON - CURRENT.scs";
                parser.WriteFile(cfgpath, newdata);
            }
            var data = parser.ReadFile(cfgpath);
            var spring = data["Seasons"][Season.Spring.ToString()];
            var summer = data["Seasons"][Season.Summer.ToString()];
            var autumn = data["Seasons"][Season.Autumn.ToString()];
            var winter = data["Seasons"][Season.Winter.ToString()];
            var current = data["Replace"]["CurrentSeason"];
            //if ()

            Console.ReadKey();
        }
        static private Season getSeason(DateTime date, bool ofSouthernHemisphere = false)
        {
            var hemisphereConst = (ofSouthernHemisphere ? 2 : 0);
            Func<int, int> getReturn = (northern) => {
                return (northern + hemisphereConst) % 4;
            };
            var value = (float)date.Month + date.Day / 100f;
            if (value < 3.21 || value >= 12.22) return (Season)getReturn(3); // Winter
            if (value < 6.21) return (Season)getReturn(0); // Spring
            if (value < 9.23) return (Season)getReturn(1); // Summer
            return (Season)getReturn(2); // Autumn
        }
    }
}
