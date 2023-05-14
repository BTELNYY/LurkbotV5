using Discord;
using Discord.WebSocket;
using LurkbotV5.BaseClasses;
using LurkbotV5.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.SlashCommands
{
    public class CommandParseUsers : CommandBase
    {
        public override string CommandName => "reparseusers";
        public override string Description => "Forces the bot to refresh user level roles.";
        public override GuildPermission RequiredPermission => GuildPermission.Administrator;

        public override async void Execute(SocketSlashCommand command)
        {
            await command.RespondAsync(TranslationManager.GetTranslations().LevelRolePhrases.ParsingUsers);
            DiscordManager.RefreshAllSavedUsers();
            await command.Channel.SendMessageAsync(TranslationManager.GetTranslations().LevelRolePhrases.DoneParse);
        }
    }
}
