using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace LurkbotV5.Managers
{
    public static class APIManager
    {
        public static bool APIDisabled = false;


        public static readonly string DevURL = @"http://localhost:8000/";
        public static readonly string ProdURL = @"http://backend:8000/";
        public static string CurrentURL { get; private set; } = "?";

        public static void Init()
        {
            if (Environment.GetEnvironmentVariable("RUNNING_IN_DOCKER") == null)
            {
                CurrentURL = DevURL;
            }
            else
            {
                CurrentURL = ProdURL;
            }
        }

        public static string TestAuth(string token)
        {
            try
            {
                string url = CurrentURL;
                string html = string.Empty;
                string requrl = "test/";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = client.GetStringAsync(url + requrl);
                response.Wait();
                string result = response.Result;
                return result;
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                return "Error";
            }
        }

        public static PlayerStats GetPlayerStats(string? input)
        {
            if(input is null)
            {
                return new PlayerStats();
            }
            bool IsID = false;
            bool IsNorthwood = false;
            bool HasAtSymbol = false;
            HasAtSymbol = input.Contains('@');
            if (HasAtSymbol && ulong.TryParse(input.Split('@')[0], out ulong id))
            {
                IsID = true;
            }
            if(input.Length == 17 && ulong.TryParse(input, out id))
            {
                IsID = true;
            }
            if (HasAtSymbol && input.Split('@')[1] == "northwood")
            {
                IsNorthwood = true;
            }

            string url = CurrentURL;
            string html = string.Empty;
            string requrl = "";

            if(IsID)
            {
                requrl = "query/id/" + input;
            }
            if(!IsID || IsNorthwood)
            {
                requrl = "query/last_nick/" + input;
            }

            var client = new HttpClient();
            Log.WriteDebug(url + requrl);
            try
            {
                var response = client.GetStringAsync(url + requrl);
                response.Wait();
                string result = response.Result;
                Log.WriteDebug(result);
                return JsonConvert.DeserializeObject<PlayerStats>(result);
            }
            catch(Exception ex)
            {
                Log.WriteError("Error when fetching player details! \n" + ex.ToString());
                PlayerStats p = new PlayerStats();
                //this is a janky way to handle this, but it works
                p.PlayTime = -1;
                return p;
            }
        }

        public static NWAllResponse GetServerStatus(string token)
        {
            try
            {
                string url = CurrentURL;
                string html = string.Empty;
                string requrl = "nw/all";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                Log.WriteDebug("GET: " +url + requrl);
                var response = client.GetStringAsync(url + requrl);
                response.Wait();
                string result = response.Result;
                Log.WriteDebug(result);
                List<ServerResponse>? list = JsonConvert.DeserializeObject<List<ServerResponse>>(result);
                if (list == null)
                {
                    Log.WriteError("Failed to parse List of responses.");
                    return new NWAllResponse();
                }
                NWAllResponse resp = new NWAllResponse();
                resp.value = list.ToArray();
                return resp;
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                return new NWAllResponse();
            }
        }

        public static PlayerStats[] GetPlaytimeLeaderboard()
        {
            try
            {
                string url = CurrentURL;
                string html = string.Empty;
                string requrl = "/query/leaderboard/";
                var client = new HttpClient();
                Log.WriteDebug("GET: " + url + requrl);
                var response = client.GetStringAsync(url + requrl);
                response.Wait();
                string result = response.Result;
                Log.WriteDebug(result);
                PlayerStats[]? list = JsonConvert.DeserializeObject<PlayerStats[]>(result);
                if(list == null)
                {
                    Log.WriteWarning("List is null!");
                    return new PlayerStats[0];
                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                return new PlayerStats[0];
            }
        }
    }

    public struct PlayerStats
    {
        [JsonProperty("id")]
        public string SteamID;
        [JsonProperty("first_seen")]
        public DateTime FirstSeen;
        [JsonProperty("last_seen")]
        public DateTime LastSeen;
        [JsonProperty("play_time")]
        public long PlayTime;
        [JsonProperty("last_nickname")]
        public string LastNickname;
        [JsonProperty("nicknames")]
        public List<string> Usernames;
        [JsonProperty("flags")]
        public List<Flags> PFlags;
        [JsonProperty("time_online")]
        public long TimeOnline;
        [JsonProperty("login_amt")]
        public uint LoginAmount;

        public PlayerStats(Player p)
        {
            SteamID = p.ID;
            LastNickname = p.Nickname;
            FirstSeen = DateTime.UtcNow;
            LastSeen = DateTime.UtcNow;
            PlayTime = 0L;
            PFlags = new List<Flags>();
            Usernames = new List<string>();
            TimeOnline = 0L;
            LoginAmount = 0;
        }
        public void ResetOnlineTime()
        {
            TimeOnline = 0;
        }
    }


    public struct Flags
    {
        public PlayerFlags Flag;
        public string Issuer;
        public DateTime IssueTime;
        public string Comment;
    }

    public enum PlayerFlags
    {
        KOS,
        MASSKOS,
        RACISM,
        CHEATING,
        CAMPING,
        MICSPAM,
        TEAMING,
        SEXUALCOMMENTS,
        REPORTABUSE,
        BITCH,
        NONE,
        SEXISM,
        HOMOPHOBIA,
        TRANSPHOBIA,
        HATESPEECH
    }

    public struct NWAllResponse
    {
        public ServerResponse[] value;
    }

    public struct ServerResponse
    {
        public bool Success;
        public int Cooldown;
        public Server[] Servers;
    }

    public struct Server
    {
        public int ID;
        public int Port;
        public bool Online;
        public Player[] PlayersList;

        public string[] GetPlayerNames()
        {
            List<string> names = new List<string>();
            foreach (Player player in PlayersList)
            {
                names.Add(player.Nickname);
            }
            return names.ToArray();
        }
    }

    public struct Player
    {
        public string ID;
        public string Nickname;
        public override string? ToString()
        {
            if (ID == null || Nickname == null)
            {
                return null;
            }
            return "Nickname: " + Nickname + "; ID: " + ID;
        }
    }
}
