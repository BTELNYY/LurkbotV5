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
    public class CommandGetWarnings : CommandBase
    {
        public override string CommandName => "getwarnings";
        public override GuildPermission RequiredPermission => base.RequiredPermission;
        public override string Description => "Get users warnings";
        public override async void Execute(SocketSlashCommand command)
        {
            base.Execute(command);
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            SocketGuildUser user = (SocketGuildUser)options[0].Value;
            ulong userId = user.Id;
            DiscordUserConfig cfg = DiscordManager.GetUserConfig(userId);
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle(TranslationManager.GetTranslations().WarningsPhrases.WarningsTitle);
            eb.AddField(TranslationManager.GetTranslations().WarningsPhrases.TotalWarningsField, cfg.UserWarnings.TotalWarnings);
            string warningsString = "```ID, Sender, SenderID, Reason \n";
            int counter = 0;
            foreach(UserWarning warn in cfg.UserWarnings.Warnings)
            {
                warningsString += $"{counter}, {warn.Sender}, {warn.SenderID}, {warn.Reason}";
                if(counter != cfg.UserWarnings.Warnings.Count)
                {
                    warningsString += "\n";
                }
                counter++;
            }
            warningsString += "```";
            eb.AddField(TranslationManager.GetTranslations().WarningsPhrases.WarningsField, warningsString);
            eb.WithCurrentTimestamp();
            await command.RespondAsync(embed: eb.Build());
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
            Options.Clear();
            Options.Add(cob);
        }
    }
}
