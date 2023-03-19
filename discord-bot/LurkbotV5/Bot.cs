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
        public APIManager? APIManager { get; private set; }
    }
}