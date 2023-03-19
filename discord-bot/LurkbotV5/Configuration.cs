using System;

namespace LurkbotV5
{
    public class Configuration
    {
        public string Token { get; set; } = "token";
        public bool DisableNonSLCommands { get; set; } = false;
        public uint RefreshCooldown { get; set; } = 60;
    }
}
