using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Commands
{
    internal class CommandDestroyAppCommands : CommandBase
    {
        public override string CommandName => "destroycommands";
        public override string Description => "Destroy ALL global application commands, then restart.";
        public override GuildPermission RequiredPermission => GuildPermission.Administrator;

        public override void Execute(SocketSlashCommand command)
        {
            command.RespondAsync("Destroying all commands.");
            Bot.Instance.GetDiscordManager().DestroyAllAppCommands();
            command.Channel.SendMessageAsync("Commands Destroyed, restarting.");
            Environment.Exit(1);
        }
    }
}
