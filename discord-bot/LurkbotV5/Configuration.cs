﻿using btelnyy.ConfigLoader.API;
using System;

namespace LurkbotV5
{
    public class Configuration
    {
        public string Token { get; set; } = "token";
        public bool DisableNonSLCommands { get; set; } = false;
        public uint RefreshCooldown { get; set; } = 60;
        public string AuthKey { get; set; } = "key";
        public ulong UpdateChannelID { get; set; } = 0;
        public ulong GuildID { get; set; } = 0;
        public Dictionary<string, string> ServerNames { get; set; } = new();

        public Configuration()
        {
            ConfigData configData = ConfigManager.GetConfiguration("./config.txt");
            Token = configData.GetString("bot_key", "key");
            DisableNonSLCommands = configData.GetBool("disable_non_sl_commands", false);
            RefreshCooldown = configData.GetUInt("refresh_cooldown", 60);
            AuthKey = configData.GetString("auth_key", "key");
            UpdateChannelID = configData.GetULong("update_channel_id", 0);
            GuildID = configData.GetULong("guild_id", 0);
            ServerNames = configData.GetDict("server_names", new Dictionary<string, string>());
        }
    }
}
