using btelnyy.ConfigLoader.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Managers
{
    public static class TranslationManager
    {
        public static readonly string DefaultTranslations = "en-us.txt";
        public static Translations Translations { get; private set; }

        private static string Path = "./translations/";

        public static void Init()
        {
            Log.WriteInfo("Starting translations system!");
            if(File.Exists(Path + Bot.Instance.GetConfig().TranslationName + ".json"))
            {
                Log.WriteInfo("File exists, loading.");
                Translations = JsonConvert.DeserializeObject<Translations>(File.ReadAllText(Path + Bot.Instance.GetConfig().TranslationName + ".json"));
            }
            else
            {
                Log.WriteWarning("Unable to find translations file, creating new based on default values (en-us)!");
                Translations = new Translations();
                string json = JsonConvert.SerializeObject(Translations, Formatting.Indented);
                File.WriteAllText(Path + Bot.Instance.GetConfig().TranslationName + ".json", json);
            }
        }

        public static Translations GetTranslations()
        {
            return Translations;
        }
    }

    public struct Translations
    {
        //generic
        public string Yes = "Yes";
        public string No = "No";
        public string Success = "Success";
        public string Error = "Error";
        public string Warning = "Warning";
        //playerdata
        public string PlayerCount = "Player Count";
        public string PlayerDetails = "Player Details: ";
        public string SteamID = "SteamID";
        public string FirstSeen = "First Seen";
        public string LastSeen = "last Seen";
        public string OldNames = "Old Names";
        public string PlayTime = "Playtime";
        public string Logins = "Logins";
        public string TimeOnline = "Time Online";
        public string Flags = "Flags";
        public string PlaytimeLeaderboard = "Playtime Leaderboard";
        //what the levels are reffered to in UI
        //level specific
        public string RankData = "Rank Data: ";
        public string LevelName = "Access Tier";
        public string XPRequiredXP = "XP/RequiredXP";
        public string XPLocked = "XP Locked";
        //userpfp
        public string HasGuildPFP = "Has Guild PFP";
        public string GuildPFPURL = "Guild PFP URL";
        public string PFPURL = "PFP URL";

        public Translations()
        {

        }
    }
}
