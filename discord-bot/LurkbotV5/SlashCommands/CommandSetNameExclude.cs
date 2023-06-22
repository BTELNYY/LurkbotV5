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
    public class CommandSetNameExclude : CommandBase
    {
        public override string CommandName => "setexcludestatus";
        public override string Description => "Set exclusion status of a name. (Excluded names are hidden from the list of players on the bot}";
        public override GuildPermission RequiredPermission => GuildPermission.Administrator;

        public async override void Execute(SocketSlashCommand command)
        {
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            string username = (string)options[0].Value;
            bool isExcluded = ServerListEmbedManager.IsNameExcluded(username);
            bool success = true;
            if (isExcluded)
            {
                success = ServerListEmbedManager.RemoveExcludedName(username);
            }
            else
            {
                success = ServerListEmbedManager.AddExcludedName(username);
            }
            if(success)
            {
                await command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Success, ephemeral: true);
            }
            else
            {
                await command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Error, ephemeral: true);
            }
        }

        public override void BuildOptions()
        {
            Options.Clear();
            CommandOptionsBase cob = new()
            {
                Name = "name",
                Description = "Name to exclude/unexclude",
                Required = true,
                OptionType = ApplicationCommandOptionType.String
            };
            Options.Add(cob);
        }
    }
}
