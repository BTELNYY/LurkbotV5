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

        public Bot(Configuration? config, DiscordSocketClient? client, DiscordManager? discordManager)
        {
            Config = config;
            Client = client;
            DiscordManager = discordManager;
        }

        public void StartBot()
        {
            if (Client == null)
            {
                Log.WriteFatal("DiscordSocketClient is null!");
                return;
            }
            if (Config == null)
            {
                Log.WriteFatal("Config is null!");
                return;
            }
            if(DiscordManager == null) 
            {
                Log.WriteFatal("DiscordManager is null!");
                return;
            }
            
            Client.LoginAsync(TokenType.Bot, Config.Token);
            Client.Log += LogEvent;
            Client.Ready += OnReady;
            DiscordManager.EventInit();
            DiscordManager.BuildInit();
            Client.StartAsync();
        }

        public Task OnReady()
        {
            return Task.CompletedTask;
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