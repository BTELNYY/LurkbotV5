using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Managers
{
    public class ServerListEmbedManager
    {
        private static List<string> ExcludedNames = new List<string>();
        private static string filename = "excluded_names.txt";
        public static bool AddExcludedName(string name)
        {
            if(ExcludedNames.Contains(name)) return false;
            ExcludedNames.Add(name);
            UpdateEmbed();
            SaveData();
            return true;
        }
        public static bool RemoveExcludedName(string name)
        {
            bool success = ExcludedNames.Remove(name);
            if (success)
            {
                UpdateEmbed();
            }
            SaveData();
            return success;
        }
        public static bool IsNameExcluded(string name)
        {
            return ExcludedNames.Contains(name);
        }
        public static void LoadData()
        {
            ExcludedNames = new List<string>();
            if (File.Exists(DiscordManager.ServerConfigPath + filename))
            {
                string data = File.ReadAllText(DiscordManager.ServerConfigPath + filename);
                foreach(string line in data.Split("\n"))
                {
                    ExcludedNames.Add(line);
                }
                SaveData();
            }
            else
            {
                Log.WriteWarning("File doesn't exist, creating new.");
                SaveData();
            }
        }
        private static void SaveData()
        {
            string data = string.Join("\n", ExcludedNames);
            Directory.CreateDirectory(DiscordManager.ServerConfigPath);
            if (File.Exists(DiscordManager.ServerConfigPath + filename))
            {
                File.Delete(DiscordManager.ServerConfigPath + filename);
                File.WriteAllText(DiscordManager.ServerConfigPath + filename, data);
            }
            else
            {
                Log.WriteInfo("Creating file as it doesn't exist.");
                File.WriteAllText(DiscordManager.ServerConfigPath + filename, data);
            }
        }

        public static async Task UpdateTask()
        {
            Log.WriteInfo("Updating Embeds");
            while (true)
            {
                UpdateEmbed();
                Log.WriteInfo("Updated Embed, waiting " + Bot.Instance.GetConfig().RefreshCooldown + "s");
                await Task.Delay(1000 * (int)Bot.Instance.GetConfig().RefreshCooldown);
            }
        }
        static async void UpdateEmbed()
        {
            Configuration config = Bot.Instance.GetConfig();
            ulong guildid = config.GuildID;
            ulong channelid = config.UpdateChannelID;
            NWAllResponse response = APIManager.GetServerStatus(Bot.Instance.GetConfig().AuthKey);

            if (response.value.Count() == 0)
            {
                Log.WriteError("Failed to fetch servers: server count is 0");
                return;
            }
            List<Embed> embeds = new List<Embed>();
            foreach (ServerResponse s in response.value)
            {
                Log.WriteDebug("Creating embeds");
                foreach (Server s1 in s.Servers)
                {
                    string name = "";
                    if (Bot.Instance.GetConfig().ServerNames.ContainsKey(s1.ID.ToString()))
                    {
                        name = Bot.Instance.GetConfig().ServerNames[s1.ID.ToString()];
                    }
                    else
                    {
                        name = "[Missing Server Name]";
                        continue;
                    }
                    var embed = new EmbedBuilder
                    {
                        Title = name
                    };
                    List<string> playerNames = s1.GetPlayerNames().ToList();
                    playerNames.RemoveAll(x => ExcludedNames.Contains(x));
                    embed.WithDescription("Players currently online:\n```\n" + string.Join("\n", playerNames) + "```")
                        .WithCurrentTimestamp()
                        .WithColor(Color.Green)
                        .AddField("Players online", playerNames.Count);
                    Log.WriteDebug("Embed created");
                    embeds.Add(embed.Build());
                }
            }
            var channel = Bot.Instance.GetClient().GetChannel(Bot.Instance.GetConfig().UpdateChannelID) as ITextChannel;
            if (channel == null)
            {
                Log.WriteFatal("Channel not found! " + Bot.Instance.GetConfig().UpdateChannelID);
                return;
            }
            Log.WriteDebug("Channel obtained");
            var meses = await channel.GetMessagesAsync().FlattenAsync();
            Log.WriteDebug("Messages obtained");
            if (meses == null)
            {
                Log.WriteWarning("No messages, cringe");
                await channel.SendMessageAsync(embeds: embeds.ToArray());
                return;
            }
            Log.WriteDebug("Searching for messages from bot");
            var botMes = meses.Where((message => message.Author.Id == Bot.Instance.GetClient().CurrentUser.Id));
            Log.WriteDebug("Getting first bot message");
            if (!botMes.Any())
            {
                Log.WriteWarning("No messages, cringe");
                await channel.SendMessageAsync(embeds: embeds.ToArray());
                return;
            }
            var messagetoEdit = botMes.First();
            Log.WriteDebug("Checking dat shit");
            if (messagetoEdit == null)
            {
                // create new message
                Log.WriteDebug("Create new message");
                await channel.SendMessageAsync(embeds: embeds.ToArray());
            }
            else
            {
                // edit message
                Log.WriteDebug("Edit message");
                var mestoEdituser = messagetoEdit as IUserMessage;
                if (mestoEdituser == null)
                {
                    Log.WriteFatal("not a IUserMessage");
                    return;
                }
                await mestoEdituser.ModifyAsync(properties => { properties.Embeds = embeds.ToArray(); });
            }
            Bot.Instance.GetDiscordManager().SetBotStatus("with " + response.value[0].Servers[0].PlayersList.Length.ToString() + " players! See server-status for more!");
        }
    }
}
