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

namespace LurkbotV5.Managers
{
    public static class APIManager
    {
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
            catch(Exception ex)
            {
                Log.WriteError(ex.ToString());
                return "Error";
            }
        }

        public static NWAllResponse GetServerStatus(string token)
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
                return JsonConvert.DeserializeObject<NWAllResponse>(result);
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                return new NWAllResponse();
            }
        }
    }

    public struct NWAllResponse
    {
        public ServerResponse value;
        public int count;
    }

    public struct ServerResponse
    {
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
            string[] strings = { };
            foreach(var player in PlayersList)
            {
                strings.Append(player.Nickname);
            }
            return strings;
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
