using System;

namespace LurkbotV5
{
    public class Configuration
    {
        public Config Config { get; private set; }
        public void LoadConfiguration(string filename)
        {

        }
    }

    public struct Config
    {
        public string Token { get; set; }
        public string Key { get; set; }
        
    }
}
