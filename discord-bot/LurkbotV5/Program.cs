using Discord.WebSocket;
using Discord;
using System;
using LurkbotV5.Managers;

namespace LurkbotV5
{
    public class Program
    {
        public static bool CustomConfigPath = false;
        public static string CustomPath = "";

        public static Task Main(string[] args) => new Program().MainAsync(args);

        public async Task MainAsync(string[] args)
        {
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
    }
}