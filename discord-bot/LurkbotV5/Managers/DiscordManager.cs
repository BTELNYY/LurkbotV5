using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using btelnyy.ConfigLoader.API;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.Net;
using LurkbotV5.Commands;
using System.Diagnostics.Metrics;
using LurkbotV5.Managers;

namespace LurkbotV5
{
    //class handles all discord specific stuff, such as leveling and commands
    public class DiscordManager
    {
        public Bot? Bot { get; private set; }
        public DiscordSocketClient Client { get; private set; }
        
        public Dictionary<string, CommandBase> Commands { get; private set; } = new Dictionary<string, CommandBase>();

        public DiscordManager(DiscordSocketClient client) 
        {
            Client = client;
        }

        public void EventInit()
        {

        }

        public void BuildInit()
        {
            
        }

        public void RepeatTaskInit()
        {
            Log.WriteInfo("Starting Repeating Task");
            Task.Run(() => UpdateTask());
        }

        async Task UpdateTask()
        {
            Log.WriteInfo("Updating Embeds");
            while (true)
            {
                UpdateEmbed();
                Log.WriteInfo("Updated Embed, waiting " + GetBot().GetConfig().RefreshCooldown + "s");
                await Task.Delay(1000 * (int)GetBot().GetConfig().RefreshCooldown);
            }
        }

        async void UpdateEmbed()
        {
            Configuration config = GetBot().GetConfig();
            ulong guildid = config.GuildID;
            ulong channelid = config.UpdateChannelID;
            NWAllResponse response = APIManager.GetServerStatus(GetBot().GetConfig().AuthKey);

            if(response.count == 0)
            {
                Log.WriteError("Failed to fetch servers: server count is 0");
                return;
            }

            foreach(Server s in response.value.Servers)
            {
                Log.WriteDebug("Creating embed");
                string name = "";
                if (GetBot().GetConfig().ServerNames.ContainsKey(s.ID.ToString()))
                {
                    name = GetBot().GetConfig().ServerNames[s.ID.ToString()];
                }
                else
                {
                    name = "[Missing Server Name]";
                }
                var embed = new EmbedBuilder
                {
                    Title = name
                };
                embed.WithDescription("Players currently online:\n```\n" + String.Join("\n", s.GetPlayerNames()) + "```")
                    .WithCurrentTimestamp()
                    .WithColor(Color.Green)
                    .AddField("Players online", s.PlayersList.Length);
                Log.WriteDebug("Embed created");
                var channel = GetBot().GetClient().GetChannel(GetBot().GetConfig().UpdateChannelID) as ITextChannel;
                if (channel == null)
                {
                    Log.WriteFatal("Channel not found! " + GetBot().GetConfig().UpdateChannelID);
                    return;
                }
                Log.WriteDebug("Channel obtained");
                var meses = await channel.GetMessagesAsync().FlattenAsync();
                Log.WriteDebug("Messages obtained");
                if (meses == null)
                {
                    Log.WriteWarning("No messages, cringe");
                    await channel.SendMessageAsync(embed: embed.Build());
                    return;
                }
                Log.WriteDebug("Searching for messages from bot");
                var botMes = meses.Where((message => message.Author.Id == GetBot().GetClient().CurrentUser.Id));
                Log.WriteDebug("Getting first bot message");
                if (!botMes.Any())
                {
                    Log.WriteWarning("No messages, cringe");
                    await channel.SendMessageAsync(embed: embed.Build());
                    return;
                }
                var messagetoEdit = botMes.First();
                Log.WriteDebug("Checking dat shit");
                if (messagetoEdit == null)
                {
                    // create new message
                    Log.WriteDebug("Create new message");
                    await channel.SendMessageAsync(embed: embed.Build());
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
                    await mestoEdituser.ModifyAsync(properties => { properties.Embed = embed.Build(); });
                }
            }
        }

        public void SetBot(Bot bot)
        {
            Bot = bot;
        }

        public Bot GetBot()
        {
            if (Bot == null)
            {
                Log.WriteFatal("Bot is null!");
                return new Bot(null, null, null);
            }
            else
            {
                return Bot;
            }
        }

        public void BuildCommand(CommandBase command)
        {
            DiscordSocketClient client = Client;
            SlashCommandBuilder scb = new();
            if(Bot == null || Bot.Config == null)
            {
                Log.WriteFatal("Bot and Config are null!");
                return;
            }
            if (Bot.Config.DisableNonSLCommands && command.CommandType != CommandType.SL)
            {
                return;
            }
            scb.WithName(command.CommandName);
            scb.WithDescription(command.Description);
            scb.WithDMPermission(command.IsDMEnabled);
            command.BuildOptions();
            foreach (CommandOptionsBase cop in command.Options)
            {
                Log.WriteDebug("Building option: " + cop.Name);
                scb.AddOption(cop.Name, cop.OptionType, cop.Description, cop.Required);
            }
            scb.DefaultMemberPermissions = command.RequiredPermission;
            scb.IsDefaultPermission = command.IsDefaultEnabled;
            command.BuildAliases();
            Commands.Add(command.CommandName, command);
            Log.WriteInfo("Registering Aliases for: " + command.CommandName + "; Alias: " + string.Join(", ", command.Aliases.ToArray()));
            foreach (string alias in command.Aliases)
            {
                Commands.Add(alias, command);
            }
            try
            {
                Log.WriteInfo("Building Command: " + command.CommandName);
                if (command.PrivateCommand)
                {
                    client.GetGuild(command.PrivateServerID).CreateApplicationCommandAsync(scb.Build());
                }
                else
                {
                    client.CreateGlobalApplicationCommandAsync(scb.Build());
                }
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Log.WriteError(json);
            }
            catch (Exception exception)
            {
                Log.WriteError("Failed to build command: " + command.CommandName + "\n Error: \n " + exception.ToString());
            }
        }
    }
}