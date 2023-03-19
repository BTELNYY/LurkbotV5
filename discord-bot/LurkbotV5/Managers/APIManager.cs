using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Managers
{
    public class APIManager
    {
        public Bot? Bot { get; private set; }

        public APIManager(Bot bot)
        {
            Bot = bot;
        }
    }
}
