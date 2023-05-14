using btelnyy.ConfigLoader.API;
using LurkbotV5.BaseClasses;
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
        public Generic GenericPhrases = new();
        public struct Generic
        {
            public string Yes = "Yes";
            public string No = "No";
            public string Success = "Success";
            public string Error = "Error";
            public string Warning = "Warning";
            public string Acknowledged = "Acknowledged";
            public string DurationField = "Length (seconds)";
            public string ReasonField = "Reason";
            public string AuthorField = "Author";
            public string NoReason = "No Reason Given";
            public Generic() { }
        }

        public Moderation ModerationPhrases = new();

        public struct Moderation
        {
            public string UserKicked = "{user} was kicked";
            public string UserBanned = "{user} was banned";

            public Moderation() { }
        }



        //playerdata
        public PlayerData PlayerDataPhrases = new();

        public struct PlayerData
        {
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

            public PlayerData() { }
        }
        //what the levels are reffered to in UI
        //level specific
        public Level LevelPhrases = new();

        public struct Level
        {
            public string RankData = "Rank Data: ";
            public string LevelName = "Access Tier";
            public string XPRequiredXP = "XP/RequiredXP";
            public string XPLocked = "XP Locked";

            public Level() { }
        }
        //userpfp
        public PFP PFPPhrases = new();

        public struct PFP
        {
            public string HasGuildPFP = "Has Guild PFP";
            public string GuildPFPURL = "Guild PFP URL";
            public string PFPURL = "PFP URL";

            public PFP() { }
        }
        //channel lockdown
        public Lockdown LockdownPhrases = new();

        public struct Lockdown
        {
            public string ChannelLockdownStarted = "This channel is locked down until further notice.";
            public string ChannelLockdownEnded = "This channel is no longer locked down.";
            public string ChannelDelayedLockdownIssued = "Channel will be locked down in 30 seconds.";
            public string ChannelDelyedLockdownStarted = "Channel is locked down and ready for decontamination, the removal of cringe has now begun.";

            public Lockdown() { }
        }


        public LevelRole LevelRolePhrases = new();
        public struct LevelRole
        {
            public string FailedParseAction = "Unable to parse requested action.";
            public string SuccessAddingLevelRole = "Added level role with no issues.";
            public string GeneralFailure = "Error occured while running command. See log.";
            public string NoSuchRoleLevel = "No role level exists for stated XP level.";
            public string SuccessRemovingLevelRole = "Removed level role with no issues.";
            public string ParsingUsers = "Parsing users and assigning roles as needed....";
            public string DoneParse = "Parse Complete.";
            

            public LevelRole()
            {

            }
        }

        public Timeout TimeoutPhrases = new();

        public struct Timeout
        {
            public string UserTimedOut = "{user} was timed out.";
            public string DurationTooShort = "You must specify a longer timeout duration.";


            public Timeout() { }
        }

        public ChannelMute ChannelMutePhrases = new();

        public struct ChannelMute
        {
            public string ChannelMuteUser = "{user} was muted in this channel.";
            public string ChannelUnmuteUser = "{user} was unmuted in this channel.";

            public ChannelMute() { } 
        }

        public DirectMessage DirectMessagePhrases = new();

        public struct DirectMessage
        {
            public string UserKickedFromGuildNoAuthor = "You have been kicked from `{server}` for `{reason}`";
            public string UserKickedFromGuild = "You have been kicked from `{server}` for `{reason}` by `{author}`";
            public string UserBannedFromGuildNoAuthor = "You have been banned from `{server}` for `{reason}`";
            public string UserBannedFromGuild = "You have been banned from `{server}` for `{reason}` by `{author}`";
            public DirectMessage() { }
        }

        public MentionCommand MentionCommandPhrases = new();

        public struct MentionCommand
        {
            public string NoSuchCommand = "No such mention command.";
            public string InvalidPermissions = "Invalid permissions for mention command.";
            public string ErrorInCommand = "Error running mention command.";
            public string DMChannelDisabled = "Unable to use mention commands in DM channels.";
            public MentionCommand() { }
        }

        public GenericError GenericErrorPhrases = new();

        //For random errors
        public struct GenericError
        {
            public string MustReplyToMessage = "You must reply to a message when using this command.";



            public GenericError() { }
        }

        public Translations()
        {

        }
    }
}
