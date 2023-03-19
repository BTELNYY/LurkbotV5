using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Commands
{
    public class CommandOptionsBase
    {
        public virtual string Name { get; set; } = "option";
        public virtual ApplicationCommandOptionType OptionType { get; set; } = ApplicationCommandOptionType.String;
        public virtual bool Required { get; set; } = false;
        public virtual string Description { get; set; } = "Desc";
    }
}
