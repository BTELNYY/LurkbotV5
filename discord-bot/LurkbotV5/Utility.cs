using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5
{
    internal class Utility
    {
        public static Embed GetErrorEmbed(string title, string message)
        {
            EmbedBuilder eb = new();
            eb.WithColor(Color.Red);
            eb.WithTitle(title);
            eb.WithDescription(message);
            return eb.Build();
        }
    }
}
