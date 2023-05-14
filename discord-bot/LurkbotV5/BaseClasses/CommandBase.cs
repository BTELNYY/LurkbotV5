using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace LurkbotV5.BaseClasses
{
    public class CommandBase
    {
        public virtual string CommandName { get; private set; } = "commandname";
        public virtual string Description { get; private set; } = "Command Description";
        public virtual bool IsDMEnabled { get; private set; } = false;
        public virtual GuildPermission RequiredPermission { get; private set; }
        public virtual List<CommandOptionsBase> Options { get; private set; } = new List<CommandOptionsBase>();
        public virtual bool IsDefaultEnabled { get; private set; } = true;
        public virtual List<string> Aliases { get; private set; } = new();
        public virtual bool PrivateCommand { get; private set; } = false;
        public virtual ulong PrivateServerID { get; private set; } = 0;
        public virtual CommandType CommandType { get; private set; } = CommandType.DISCORD;
        public virtual void Execute(SocketSlashCommand command)
        {

        }
        public virtual void BuildAliases()
        {

        }
        public virtual void BuildOptions()
        {

        }

        public virtual SocketSlashCommandDataOption[] GetOptionsOrdered(List<SocketSlashCommandDataOption> options)
        {
            SocketSlashCommandDataOption[] array = new SocketSlashCommandDataOption[Options.Count];
            foreach (var option in options)
            {
                int counter = 0;
                foreach (var optionbase in Options)
                {
                    if (optionbase != null)
                    {
                        if (optionbase.Name == option.Name)
                        {
                            if (array[counter] != null)
                            {
                                Log.WriteError("Command options have been duplicated.");
                            }
                            array[counter] = option;
                        }
                    }
                    counter++;
                }
            }
            return array;
        }
    }

    public enum CommandType
    {
        DISCORD,
        SL
    }
}
