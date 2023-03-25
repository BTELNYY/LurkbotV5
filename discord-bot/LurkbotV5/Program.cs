using Discord.WebSocket;
using Discord;
using System;

namespace LurkbotV5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DiscordSocketConfig cfg = new()
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 50
            };
            Configuration config = new();
            DiscordSocketClient client = new(cfg);
            DiscordManager discordManager = new(client);
            Bot bot = new(config, client, discordManager);
            discordManager.SetBot(bot);
            bot.StartBot();
        }
    }
}