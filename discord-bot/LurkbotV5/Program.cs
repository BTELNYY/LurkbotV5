using Discord.WebSocket;
using Discord;
using System;
using LurkbotV5.Managers;
using System.Diagnostics;

namespace LurkbotV5
{
    public class Program
    {
        public static bool CustomConfigPath = false;
        public static string CustomPath = "";
        static Thread ConsoleThread = new(HandleConsole);

        public static Task Main(string[] args) => new Program().MainAsync(args);

        public async Task MainAsync(string[] args)
        {
            ConsoleThread.Start();
            if (args.Contains("--config") && args.Length >= 2)
            {
                CustomConfigPath = true;
                CustomPath = args[args.ToList().FindIndex(x => x == "--config") + 1];
            }
            DiscordSocketConfig cfg = new()
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 50
            };
            Configuration config = new();
            DiscordSocketClient client = new(cfg);
            DiscordManager discordManager = new(client);
            APIManager.Init();
            Bot bot = new(config, client, discordManager);
            discordManager.SetBot(bot);
            bot.StartBot();
            TranslationManager.Init();
            await Task.Delay(-1);
        }

        public static void HandleConsole()
        {
            Log.WriteInfo("Started console!");
            while(ConsoleThread.IsAlive)
            {
                string? input = Console.ReadLine();
                if(string.IsNullOrEmpty(input))
                {
                    Log.WriteError("Invalid command.");
                    continue;
                }
                string[] commandParts = input.Split(" ");
                string command = commandParts[0];
                switch (command)
                {
                    case "addlevelrole":
                        Log.WriteInfo("Adding level role...");
                        DiscordManager.AddLevelRole(1, 1033154857948950588, RoleLevelActions.ADD);
                        break;
                }
                continue;
            }
        }
    }
}