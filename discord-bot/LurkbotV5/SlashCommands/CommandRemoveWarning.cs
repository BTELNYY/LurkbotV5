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
    public class CommandRemoveWarning : CommandBase
    {
        public override string CommandName => "removewarning";
        public override GuildPermission RequiredPermission => GuildPermission.MuteMembers;
        public override string Description => "Remove warning from user";
        public override List<CommandOptionsBase> Options => new();
        public override async void Execute(SocketSlashCommand command)
        {
            base.Execute(command);
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            SocketGuildUser user = (SocketGuildUser)options[0].Value;
            long id = (long)options[1].Value;
            DiscordUserConfig cfg = DiscordManager.GetUserConfig(user.Id);
            bool success = cfg.UserWarnings.RemoveWarning((int)id);
            if (success)
            {
                EmbedBuilder eb = new();
                eb.WithTitle(TranslationManager.GetTranslations().GenericPhrases.Success);
                eb.WithDescription(TranslationManager.GetTranslations().WarningsPhrases.WarningRemoved);
                eb.WithCurrentTimestamp();
                await command.RespondAsync(embed: eb.Build());
            }
            else
            {
                EmbedBuilder eb = new();
                eb.WithTitle(TranslationManager.GetTranslations().GenericPhrases.Error);
                eb.WithDescription(TranslationManager.GetTranslations().WarningsPhrases.NoWarningOnId);
                eb.WithCurrentTimestamp();
                await command.RespondAsync(embed: eb.Build());
            }
        }
        
        public override void BuildOptions()
        {
            base.BuildOptions();
            CommandOptionsBase cob = new CommandOptionsBase()
            {
                Name = "user",
                Description = "Target User",
                OptionType = ApplicationCommandOptionType.User,
                Required = true
            };
            CommandOptionsBase cob1 = new CommandOptionsBase()
            {
                Name = "index",
                Description = "Index to remove (Zero based)",
                OptionType = ApplicationCommandOptionType.Integer,
                Required = true
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob1);
        }
    }
}
