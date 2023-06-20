using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using btelnyy.ConfigLoader.API;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.Net;
using LurkbotV5.SlashCommands;
using System.Diagnostics.Metrics;
using LurkbotV5.Managers;
using System.Security.Cryptography.X509Certificates;
using LurkbotV5.BaseClasses;
using LurkbotV5.MentionCommands;
using System.Net.WebSockets;

namespace LurkbotV5
{
    //class handles all discord specific stuff, such as leveling and commands
    public class DiscordManager
    {
        public Bot? Bot { get; private set; }
        public DiscordSocketClient Client { get; private set; }

        public static Dictionary<ulong, DiscordUserConfig> UserCache = new();

        public static Dictionary<ulong, string> LastMessageCache = new();

        public static List<ulong> DoNotNotifyGhostPingCache = new();

        public static DiscordConfig DiscordConfig { get; private set; }

        public static LevelRoles LevelRoles { get; private set; }
        public static Dictionary<string, CommandBase> Commands { get; private set; } = new Dictionary<string, CommandBase>();
        public static Dictionary<string, MentionCommandBase> MentionCommands { get; private set; } = new Dictionary<string, MentionCommandBase>();

        public static readonly string UserConfigPath = "./statistics/users/";

        public static readonly string ServerConfigPath = $"./statistics/server/{Bot.Instance.GetConfig().GuildID}/";

        public DiscordManager(DiscordSocketClient client)
        {
            Client = client;
        }
        public void SetBotStatus(string message)
        {
            GetBot().GetClient().SetActivityAsync(new Game(message));
        }
        public void EventInit()
        {
            Client.SlashCommandExecuted += SlashCommandHandler;
            Client.MessageDeleted += OnMessageDeleted;
            Client.MessageDeleted += OnGhostPinging;
            Client.MessageReceived += LevelUpMessageEvent;
            Client.MessageReceived += OnMentionCommand;
            Client.UserJoined += OnUserJoin;
            Client.UserBanned += OnUserBanned;
        }
        public void DiscordConfigInit()
        {
            string guildid = GetBot().GetConfig().GuildID.ToString();
            if(!Directory.Exists(ServerConfigPath + guildid))
            {
                Directory.CreateDirectory(ServerConfigPath + guildid);
            }
            if (File.Exists(ServerConfigPath + "config.json"))
            {
                string json = File.ReadAllText(ServerConfigPath + "config.json");
                DiscordConfig = JsonConvert.DeserializeObject<DiscordConfig>(json);
            }
            else
            {
                GenerateServerConfig();
            }
            if (File.Exists(ServerConfigPath + "level_roles.json"))
            {
                string json = File.ReadAllText(ServerConfigPath + "level_roles.json");
                LevelRoles = JsonConvert.DeserializeObject<LevelRoles>(json);
            }
            else
            {
                string json = JsonConvert.SerializeObject(new LevelRoles());
                File.WriteAllText(ServerConfigPath + "level_roles.json", json);
            }
        }
        public void CommandInit()
        {
            BuildCommand(new CommandGetPlayerStats());
            BuildCommand(new CommandGetPFP());
            BuildCommand(new CommandPing());
            BuildCommand(new CommandPlayers());
            BuildCommand(new CommandRank());
            BuildCommand(new CommandDestroyAppCommands());
            BuildCommand(new CommandLeaderboard());
            BuildCommand(new CommandLockdown());
            BuildCommand(new CommandTimeout());
            BuildCommand(new CommandAddLevelRole());
            BuildCommand(new CommandRemoveLevelRole());
            BuildCommand(new CommandParseUsers());
            BuildCommand(new CommandChannelMute());
            BuildCommand(new CommandPurge());
            BuildCommand(new CommandBackupChannel());
            BuildCommand(new CommandKickUser());
            BuildCommand(new CommandCrushSkull());
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
                    if (GetBot().GetConfig().ServerNames.ContainsKey(s1.ID.ToString()))
                    {
                        name = GetBot().GetConfig().ServerNames[s1.ID.ToString()];
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
                    embed.WithDescription("Players currently online:\n```\n" + string.Join("\n", s1.GetPlayerNames()) + "```")
                        .WithCurrentTimestamp()
                        .WithColor(Color.Green)
                        .AddField("Players online", s1.PlayersList.Length);
                    Log.WriteDebug("Embed created");
                    embeds.Add(embed.Build());
                }
            }
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
                await channel.SendMessageAsync(embeds: embeds.ToArray());
                return;
            }
            Log.WriteDebug("Searching for messages from bot");
            var botMes = meses.Where((message => message.Author.Id == GetBot().GetClient().CurrentUser.Id));
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
            SetBotStatus("with " + response.value[0].Servers[0].PlayersList.Length.ToString() + " players! See server-status for more!");
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
            if (Bot == null || Bot.Config == null)
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
        public void BuildCommand(MentionCommandBase command)
        {
            bool success = MentionCommands.TryAdd(command.Command.ToLower(), command);
            if (!success)
            {
                Log.WriteError("Failed to add MentionCommand to dict. Command: " + command.Command.ToLower());
            }
            Log.WriteDebug(MentionCommands.Count.ToString());
        }
        public Task OnMentionCommand(SocketMessage msg)
        {
            IMessage message = msg as IMessage;
            if (!message.MentionedUserIds.Contains(GetBot().GetClient().CurrentUser.Id))
            {
                Log.WriteDebug("Message does not mention bot, aborting...");
                return Task.CompletedTask;
            }
            List<string> parts = msg.Content.Split(" ").ToList();
            if(parts[0] != $"<@{GetBot().GetClient().CurrentUser.Id}>")
            {
                Log.WriteDebug("Bot isnt mentioned in the first part of the message, aborting...");
                return Task.CompletedTask;
            }
            parts.RemoveAt(0);
            string command = string.Join(" ", parts);
            if (!MentionCommands.ContainsKey(command))
            {
                if(parts.Count <= 0)
                {
                    Log.WriteDebug("Empty ping brace, aborting...");
                    return Task.CompletedTask;
                }
                Log.WriteDebug("Can't find command, aborting....");
                msg.Channel.SendMessageAsync(TranslationManager.GetTranslations().MentionCommandPhrases.NoSuchCommand, messageReference: msg.Reference);
                return Task.CompletedTask;
            }
            MentionCommandBase commandBase = MentionCommands[command];
            if (msg.Channel is SocketDMChannel)
            {
                Log.WriteDebug("DM command, cant reply. Aborting....");
                msg.Channel.SendMessageAsync(TranslationManager.GetTranslations().MentionCommandPhrases.DMChannelDisabled, messageReference: msg.Reference);
                return Task.CompletedTask;
            }
            SocketGuildUser sender = (SocketGuildUser) msg.Author;
            SocketGuildChannel channel = (SocketGuildChannel)msg.Channel;
            var guild = channel.Guild;
            bool allowed = false;
            foreach(var role in sender.Roles)
            {
                if (role.Permissions.Has(commandBase.Permission))
                {
                    allowed = true;
                }
            }
            if(!allowed)
            {
                Log.WriteDebug("Invalid permissions, aborting...");
                msg.Channel.SendMessageAsync(TranslationManager.GetTranslations().MentionCommandPhrases.InvalidPermissions, messageReference: msg.Reference);
                return Task.CompletedTask;
            }
            Log.WriteDebug("Executing....");
            MentionCommandParams param = new MentionCommandParams(sender, (ISocketMessageChannel) channel, msg, guild, command);
            Task.Run(() =>
            {
                commandBase.Execute(param);
            });
            return Task.CompletedTask;
        }
        public static Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (!Commands.ContainsKey(command.CommandName))
            {
                Log.WriteError("Command Not registered in Dict: " + command.CommandName);
                command.RespondAsync("Sorry, this command is not registered internally, contact the developer about this.");
                var result = Bot.Instance.GetClient().GetGlobalApplicationCommandAsync(command.CommandId);
                result.AsTask().Wait();
                result.Result.DeleteAsync().Wait();
                return Task.CompletedTask;
            }
            try
            {
                Commands[command.CommandName].Execute(command);
            }
            catch (Exception ex)
            {
                Log.WriteError("Executing Command " + command.CommandName + " threw an exception: \n" + ex.ToString());
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
        public void DestroyAllAppCommands()
        {
            var commands = GetBot().GetClient().GetGlobalApplicationCommandsAsync();
            commands.Wait();
            foreach (var thing in commands.Result)
            {
                Log.WriteDebug("Destroying command: " + thing.Name);
                thing.DeleteAsync().Wait();
            }
        }
        public Task LevelUpMessageEvent(SocketMessage msg)
        {
            if (GetBot().GetConfig().DisableNonSLCommands)
            {
                return Task.CompletedTask;
            }
            if (msg.Author.IsBot)
            {
                return Task.CompletedTask;
            }
            if (!CheckMessage(msg.Content, msg.Author.Id))
            {
                return Task.CompletedTask;
            }
            if (LastMessageCache.ContainsKey(msg.Author.Id))
            {
                LastMessageCache[msg.Author.Id] = msg.Content;
            }
            else
            {
                LastMessageCache.Add(msg.Author.Id, msg.Content);
            }
            Random r = new();
            float num = r.Next(1, 100);
            if (num > DiscordConfig.XPEarnChance)
            {
                return Task.CompletedTask;
            }
            else
            {
                DiscordUserConfig cfg = GetUserConfig(msg.Author.Id);
                if (cfg.LockXP)
                {
                    return Task.CompletedTask;
                }
                float xpsum = r.Next((int)DiscordConfig.MinXPPerMessage, (int)DiscordConfig.MaxXPPerMessage);
                xpsum += r.NextSingle();
                uint level = cfg.XPLevel;
                float XP = cfg.XP;
                float requiredXP = GetXPPerLevel(level);
                float XPCalculated = (XP + xpsum) * DiscordConfig.XPMultiplier;
                bool levelIncreased = false;
                Log.WriteDebug("Checking XP stats now");
                if (XPCalculated > requiredXP)
                {
                    levelIncreased = true;
                    level++;
                    XP = XPCalculated - requiredXP;
                }
                else
                {
                    XP = XPCalculated;
                }
                cfg.XP = XP;
                cfg.XPLevel = level;
                Log.WriteDebug($"XP: {XP}, Level: {level}");
                //speeds up calculations for later.
                if (DiscordConfig.XPLevels.ContainsKey(level))
                {
                    DiscordConfig.XPLevels.Remove(level);
                }
                DiscordConfig.XPLevels.Add(level, requiredXP);
                Log.WriteDebug("Setting XP level to dict..");
                SetUserConfig(cfg);
                if (!levelIncreased)
                {
                    return Task.CompletedTask;
                }
                SocketGuildUser user = (SocketGuildUser)msg.Author;
                if (!LevelRoles.RoleLevels.ContainsKey(level))
                {
                    return Task.CompletedTask;
                }
                else
                {
                    foreach (RoleLevel role in LevelRoles.RoleLevels[level])
                    {
                        ulong roleid = role.RoleID;
                        RoleLevelActions action = role.Action;
                        switch (action)
                        {
                            case RoleLevelActions.ADD:
                                user.AddRoleAsync(roleid);
                                break;
                            case RoleLevelActions.REMOVE:
                                user.RemoveRoleAsync(roleid);
                                break;
                            default:
                                return Task.CompletedTask;
                        }
                        return Task.CompletedTask;
                    }
                }
            }
            return Task.CompletedTask;
        }
        public Task OnUserBanned(SocketUser user, SocketGuild guild)
        {
            DeleteUserConfig(user.Id, guild.Id);
            return Task.CompletedTask;
        }
        public Task OnUserJoin(SocketGuildUser user)
        {
            ulong id = user.Id;
            DiscordUserConfig config = GetUserConfig(id);
            foreach (uint level in LevelRoles.RoleLevels.Keys)
            {
                foreach (RoleLevel role in LevelRoles.RoleLevels[level])
                {
                    if (config.XPLevel < level)
                    {
                        continue;
                    }
                    else
                    {
                        if (role.Action == RoleLevelActions.REMOVE)
                        {
                            SocketGuild guild = Bot.Instance.GetClient().GetGuild(Bot.Instance.GetConfig().GuildID);
                            SocketRole grole = guild.GetRole(role.RoleID);
                            if (user != null && user.Roles.Contains(grole))
                            {
                                user.RemoveRoleAsync(grole);
                            }
                        }
                        else if (role.Action == RoleLevelActions.ADD)
                        {
                            SocketGuild guild = Bot.Instance.GetClient().GetGuild(Bot.Instance.GetConfig().GuildID);
                            SocketRole grole = guild.GetRole(role.RoleID);
                            if (user != null && grole != null && !user.Roles.Contains(grole))
                            {
                                user.AddRoleAsync(grole);
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
        public Task OnGhostPinging(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
        {
            Log.WriteDebug("OnGhostPing event triggered");
            if (msg.Value == null)
            {
                Log.WriteWarning("OnGhostPing msg is null!");
                return Task.CompletedTask;
            }
            if (msg.Value.Author.Id == GetBot().GetClient().CurrentUser.Id)
            {
                return Task.CompletedTask;
            }
            if (DoNotNotifyGhostPingCache.Contains(msg.Value.Id))
            {
                return Task.CompletedTask;
            }
            if (msg.Value.MentionedUserIds.Count > 0)
            {
                if (msg.Value.MentionedUserIds.Count == 1 && (msg.Value.MentionedUserIds.First() == msg.Value.Author.Id || msg.Value.MentionedUserIds.Contains(GetBot().GetClient().CurrentUser.Id)))
                {
                    return Task.CompletedTask;
                }
                //has ghost pings
                string message = "";
                foreach (ulong id in msg.Value.MentionedUserIds)
                {
                    if (id == GetBot().GetClient().CurrentUser.Id)
                    {
                        continue;
                    }
                    message += ("<@" + id.ToString() + ">");
                    message += " ";
                }
                Log.WriteDebug("Mentioned: " + msg.Value.MentionedUserIds.Count);
                EmbedBuilder eb = new();
                eb.WithTitle("uh oh, you're getting ghost pinged!");
                eb.WithCurrentTimestamp();
                eb.AddField("Content", msg.Value.Content);
                eb.AddField("Author", "<@" + msg.Value.Author.Id + ">");
                eb.Color = Color.Blue;
                channel.Value.SendMessageAsync(message, embed: eb.Build());
            }
            return Task.CompletedTask;
        }
        public Task OnMessageDeleted(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
        {
            if (msg.Value == null)
            {
                Log.WriteWarning("OnMessageDeleted msg is null!");
                return Task.CompletedTask;
            }
            if (msg.Value.Author.Id == GetBot().GetClient().CurrentUser.Id)
            {
                return Task.CompletedTask;
            }
            var channel1 = GetBot().GetClient().GetChannel(GetBot().GetConfig().DeletedMessagesChannelID) as ITextChannel;
            if (channel1 == null)
            {
                Log.WriteFatal("Channel not found! " + GetBot().GetConfig().DeletedMessagesChannelID);
                return Task.CompletedTask;
            }

            EmbedBuilder eb = new();
            eb.WithTitle("Deleted Message");
            eb.AddField("Author", "<@" + msg.Value.Author.Id + ">");
            eb.AddField("Channel", "<#" + channel.Id + ">");
            if (msg.Value.Content != null || !string.IsNullOrEmpty(msg.Value.Content))
            {
                if(msg.Value.Content.Length > 0)
                {
                    eb.AddField("Content (text)", msg.Value.Content);
                }
            }
            if (msg.Value.Attachments.Count > 0)
            {
                string atturls = "";
                foreach (var att in msg.Value.Attachments)
                {
                    string attachmentparsed = att.Url.Replace("media", "cdn").Replace("net", "com");
                    Log.WriteDebug(attachmentparsed);
                    Log.WriteDebug(att.Url);
                    if(att == msg.Value.Attachments.Last())
                    {
                        atturls += attachmentparsed;
                    }
                    else
                    {
                        atturls += attachmentparsed + "\n";
                    }
                }
                eb.AddField("Attachments", atturls);
                Embed[] embeds = { eb.Build() };
                channel1.SendMessageAsync(embeds: embeds);
            }
            else
            {
                channel1.SendMessageAsync(embed: eb.Build());
            }
            return Task.CompletedTask;
        }

        #region Rank and UserConfig
        public static bool AddLevelRole(uint level, ulong roleid, RoleLevelActions action)
        {
            Log.WriteDebug($"Adding Level role. Level: {level} roleid: {roleid}, action: {action}");
            if (LevelRoles.RoleLevels.ContainsKey(level))
            {
                if (LevelRoles.RoleLevels[level].Contains(new RoleLevel(roleid, action)))
                {
                    Log.WriteDebug("Role already exists.");
                    return false;
                }
                Log.WriteDebug("Role does not exist, adding....");
                LevelRoles.RoleLevels[level].Add(new RoleLevel(roleid, action));
                return true;
            }
            else
            {
                Log.WriteDebug("Role levels does not contain level.");
                List<RoleLevel> levelRoleList = new()
                {
                    new RoleLevel(roleid, action)
                };
                LevelRoles.RoleLevels.Add(level, levelRoleList);
                try
                {
                    WriteLevelRoleConfig();
                    return true;
                }
                catch(Exception ex)
                {
                    Log.WriteError("Failed to write config: " + ex.ToString());
                    return false;
                }
            }
        }
        public static bool RemoveLevelRole(uint level, ulong roleid)
        {
            if (!LevelRoles.RoleLevels.ContainsKey(level))
            {
                return false;
            }
            else
            {
                foreach (uint levels in LevelRoles.RoleLevels.Keys)
                {
                    if (LevelRoles.RoleLevels[levels].Count == 1)
                    {
                        LevelRoles.RoleLevels.Remove(levels);
                        if (levels == level)
                        {
                            return true;
                        }
                    }
                    foreach (RoleLevel rlvl in LevelRoles.RoleLevels[levels])
                    {
                        if (rlvl.RoleID == roleid)
                        {
                            LevelRoles.RoleLevels[levels].Remove(rlvl);
                        }
                    }
                }
                try
                {
                    WriteLevelRoleConfig();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static bool RoleLevelExists(uint value)
        {
            return LevelRoles.RoleLevels.ContainsKey(value);
        }
        public static DiscordUserConfig GetUserConfig(ulong UserID)
        {
            string userpath = UserConfigPath + UserID.ToString() + ".json";
            if (UserCache.ContainsKey(UserID))
            {
                return UserCache[UserID];
            }
            if (!File.Exists(userpath))
            {
                DiscordUserConfig config = new(UserID);
                File.WriteAllText(userpath, JsonConvert.SerializeObject(config));
                UserCache.Add(UserID, config);
                return config;
            }
            else
            {
                DiscordUserConfig config = JsonConvert.DeserializeObject<DiscordUserConfig>(File.ReadAllText(userpath));
                UserCache.Add(UserID, config);
                return config;
            }
        }
        public static void SetUserConfig(DiscordUserConfig config)
        {
            ulong id = config.UserID;
            if (UserCache.ContainsKey(id))
            {
                UserCache.Remove(id);
            }
            UserCache.Add(id, config);
            string userpath = UserConfigPath + config.UserID.ToString() + ".json";
            File.WriteAllText(userpath, JsonConvert.SerializeObject(config));
        }
        public void DeleteUserConfig(ulong id, ulong guildid)
        {
            if (guildid == GetBot().GetConfig().GuildID)
            {
                try
                {
                    File.Delete(UserConfigPath + id.ToString() + ".json");
                }
                catch (Exception ex)
                {
                    Log.WriteError("Failed to delete banned user's config data! Error: \n " + ex.ToString());
                }
            }
        }
        public static void RefreshAllSavedUsers()
        {
            Log.WriteInfo("Prepping to Enumrate all discord config saves... Total Saves: " + Directory.EnumerateFiles(UserConfigPath).ToList().Count);
            uint counter = 0;
            foreach (string file in Directory.EnumerateFiles(UserConfigPath))
            {
                DiscordUserConfig config = JsonConvert.DeserializeObject<DiscordUserConfig>(File.ReadAllText(file));
                foreach (uint level in LevelRoles.RoleLevels.Keys)
                {
                    foreach (RoleLevel role in LevelRoles.RoleLevels[level])
                    {
                        if (config.XPLevel < level)
                        {
                            continue;
                        }
                        else
                        {
                            if (role.Action == RoleLevelActions.REMOVE)
                            {
                                SocketGuild guild = Bot.Instance.GetClient().GetGuild(Bot.Instance.GetConfig().GuildID);
                                SocketGuildUser user = guild.GetUser(config.UserID);
                                SocketRole grole = guild.GetRole(role.RoleID);
                                if (user != null && user.Roles.Contains(grole))
                                {
                                    user.RemoveRoleAsync(grole);
                                }
                            }
                            else if (role.Action == RoleLevelActions.ADD)
                            {
                                SocketGuild guild = Bot.Instance.GetClient().GetGuild(Bot.Instance.GetConfig().GuildID);
                                SocketGuildUser user = guild.GetUser(config.UserID);
                                SocketRole grole = guild.GetRole(role.RoleID);
                                if (user != null && grole != null && !user.Roles.Contains(grole))
                                {
                                    user.AddRoleAsync(grole);
                                }
                            }
                        }
                    }
                }
                counter++;
                Log.WriteInfo($"Finished Parsing {counter} out of {Directory.EnumerateFiles(UserConfigPath).ToList().Count} user config files");
            }
        }
        private static bool CheckMessage(string message, ulong userid)
        {
            switch (DiscordConfig.XPRequirementStrictness)
            {
                case 0:
                    return true;
                case 1:
                    if (message.Length >= DiscordConfig.MinMessageLength)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 2:
                    if (message.Length <= DiscordConfig.MinMessageLength)
                    {
                        return false;
                    }
                    if (LastMessageCache.ContainsKey(userid) && LastMessageCache[userid] == message)
                    {
                        return false;
                    }
                    if (!message.Contains(' '))
                    {
                        return false;
                    }
                    return true;
                default:
                    return true;
            }
        }
        private static void GenerateServerConfig()
        {
            DiscordConfig config = new(true, 1);
            string json = JsonConvert.SerializeObject(config);
            File.WriteAllText(ServerConfigPath + "config.json", json);
            DiscordConfig = config;
        }
        public static float GetXPPerLevel(uint level)
        {
            float result = ((level * level) + DiscordConfig.XPOffsetToAdd) * DiscordConfig.XPPerLevelMultiplier;
            return result;
        }
        public static uint GetLevelPerXP(float xp)
        {
            float result = ((xp / xp) - DiscordConfig.XPOffsetToAdd) / DiscordConfig.XPPerLevelMultiplier;
            return (uint)Math.Round((double)result, MidpointRounding.ToZero);
        }
        private static void WriteLevelRoleConfig()
        {
            Log.WriteDebug("Writing Role Level Config!");
            if(File.Exists(ServerConfigPath + "role_levels.json"))
            {
                Log.WriteDebug("Fille exists, deleting and recreating.");
                File.Delete(ServerConfigPath + "role_levels.json");
                string json = JsonConvert.SerializeObject(LevelRoles);
                File.WriteAllText(ServerConfigPath + "role_levels.json", json);
            }
            else
            {
                Log.WriteDebug("File does not exist, writing data.");
                string json = JsonConvert.SerializeObject(LevelRoles);
                File.WriteAllText(ServerConfigPath + "role_levels.json", json);
            }
        }

        #endregion
    }

    //Structs

    #region Discord Config / User config
    public struct DiscordUserConfig
    {
        public ulong UserID;
        public uint XPLevel;
        public float XP;
        public bool LockXP;
        public DiscordUserConfig(ulong UserID, uint XPLevel = 0, float XP = 0f, bool LockXP = false)
        {
            this.UserID = UserID;
            this.XPLevel = XPLevel;
            this.XP = XP;
            this.LockXP = LockXP;
        }
    }
    public struct DiscordConfig
    {
        public bool XPEnabled;
        public float XPPerLevelMultiplier;
        //mapping is level, xp amount
        public Dictionary<uint, float> XPLevels;
        public float MaxXPPerMessage;
        public float MinXPPerMessage;
        public float XPEarnChance;
        public float XPMultiplier;
        public uint MinMessageLength;
        public uint XPOffsetToAdd;
        public short XPRequirementStrictness;
        public DiscordConfig(bool XPEnabled = true, float XPPerLevelMultiplier = 1, float MaxXPPerMessage = 7, float MinXPPerMessage = 2, float XPEarnChance = 20, uint MinMessageLength = 10, uint XPOffsetToAdd = 50, float XPMuliplier = 1, short XPRequirementStrictness = 1)
        {
            this.XPEnabled = XPEnabled;
            this.XPPerLevelMultiplier = XPPerLevelMultiplier;
            XPLevels = new();
            XPMultiplier = XPMuliplier;
            this.MinMessageLength = MinMessageLength;
            this.MaxXPPerMessage = MaxXPPerMessage;
            this.XPEarnChance = XPEarnChance;
            this.MinXPPerMessage = MinXPPerMessage;
            this.XPOffsetToAdd = XPOffsetToAdd;
            this.XPRequirementStrictness = XPRequirementStrictness;
        }
    }
    #endregion

    #region Level Roles
    public struct LevelRoles
    {
        public bool Enable = false;
        public Dictionary<uint, List<RoleLevel>> RoleLevels = new();
        public LevelRoles(Dictionary<uint, List<RoleLevel>> RoleLevels, bool Enable = true)
        {
            this.RoleLevels = RoleLevels;
            this.Enable = Enable;
        }
    }
    public struct RoleLevel
    {
        public ulong RoleID;
        public RoleLevelActions Action;
        public RoleLevel(ulong RoleID, RoleLevelActions Action)
        {
            this.RoleID = RoleID;
            this.Action = Action;
        }
    }
    public enum RoleLevelActions
    {
        ADD,
        REMOVE,
    }
    #endregion
}