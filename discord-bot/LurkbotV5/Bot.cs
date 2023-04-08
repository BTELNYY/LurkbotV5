using Discord;
using Discord.WebSocket;
using LurkbotV5.Managers;
using System;

namespace LurkbotV5
{
    public class Bot
    {
        public Configuration? Config { get; private set; }
        public DiscordSocketClient? Client { get; private set; }
        public DiscordManager? DiscordManager { get; private set; }

        public static Bot Instance { get; private set; }

        public Bot(Configuration? config, DiscordSocketClient? client, DiscordManager? discordManager)
        {
            Config = config;
            Client = client;
            DiscordManager = discordManager;
        }

        public Configuration GetConfig()
        {
            if(Config == null)
            {
                Log.WriteError("Config is null!");
                return new Configuration();
            }
            else
            {
                return Config;
            }
        }

        public DiscordManager GetDiscordManager()
        {
            if(DiscordManager == null)
            {
                Log.WriteFatal("DiscordManager is null!");
                return new DiscordManager(GetClient());
            }
            else
            {
                return DiscordManager;
            }
        }

        public DiscordSocketClient GetClient()
        {
            if(Client == null) 
            {
                Log.WriteFatal("Client is null!");
                return new DiscordSocketClient();
            }
            else
            {
                return Client;
            }
        }

        public async void StartBot()
        {
            //singleton
            if(Instance != null && Instance != this)
            {
                return;
            }
            else
            {
                Instance = this;
            }
            Log.WriteInfo("Starting Bot...");
            if (Client == null)
            {
                Log.WriteError("DiscordSocketClient is null!");
                return;
            }
            Log.WriteDebug("Client isn't null.");
            if (Config == null)
            {
                Log.WriteError("Config is null!");
                return;
            }
            Log.WriteDebug("Config isn't null.");
            if (DiscordManager == null)
            {
                Log.WriteError("DiscordManager is null!");
                return;
            }
            Log.WriteDebug("Manager isn't null.");
            Client.Log += LogEvent;
            Client.Ready += OnReady;
            Log.WriteInfo("Logging Into Discord...");
            Log.WriteDebug("Token: " + Config.Token);
            await Client.LoginAsync(TokenType.Bot, Config.Token);
            Log.WriteInfo("Starting.");
            await Client.StartAsync();
            Log.WriteInfo("Start complete, delaying task");
            await Task.Delay(-1);
        }

        public async Task OnReady()
        {
            Log.WriteInfo("Ready!");
            if (DiscordManager == null)
            {
                Log.WriteError("DiscordManager is null!");
                return;
            }
            DiscordManager.EventInit();
            DiscordManager.DiscordConfigInit();
            DiscordManager.CommandInit();
            DiscordManager.RepeatTaskInit();
        }


        private static Task LogEvent(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Error:
                    Log.WriteError(msg.Message + "\n" + msg.Exception);
                    break;
                case LogSeverity.Info:
                    Log.WriteInfo(msg.Message + "\n" + msg.Exception);
                    break;
                case LogSeverity.Warning:
                    Log.WriteWarning(msg.Message + "\n" + msg.Exception);
                    break;
                case LogSeverity.Verbose:
                    Log.WriteVerbose(msg.Message + "\n" + msg.Exception);
                    break;
                case LogSeverity.Critical:
                    Log.WriteCritical(msg.Message + "\n" + msg.Exception);
                    break;
                case LogSeverity.Debug:
                    Log.WriteDebug(msg.Message + "\n" + msg.Exception);
                    break;
                default:
                    Log.WriteWarning("The bellow message failed to be caught by any switch, default warning used.");
                    Log.WriteWarning(msg.Message + "\n" + msg.Exception);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}