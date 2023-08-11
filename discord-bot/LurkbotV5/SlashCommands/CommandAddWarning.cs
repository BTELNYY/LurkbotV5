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
    public class CommandAddWarning : CommandBase
    {
        public override string CommandName => "warn";
        public override GuildPermission RequiredPermission => GuildPermission.MuteMembers;
        public override CommandType CommandType => CommandType.DISCORD;
        public override string Description => "Warn a user.";
        public override async void Execute(SocketSlashCommand command)
        {
            base.Execute(command);
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            SocketGuildUser user = (SocketGuildUser)options[0].Value;
            string reason = (string)options[1].Value;
            bool senddm = (bool)options[2].Value;
            try
            {
                if (senddm)
                {
                    await user.SendMessageAsync(TranslationManager.GetTranslations().DirectMessagePhrases.UserWarnedInGuild.Replace("{server}", Bot.Instance.GetClient().GetGuild((ulong)command.GuildId).Name).Replace("{reason}", reason).Replace("{author}", command.User.Username));
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
            }
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithCurrentTimestamp();
            eb.WithTitle(TranslationManager.GetTranslations().ModerationPhrases.UserWarned.Replace("{user}", user.Username));
            eb.AddField(TranslationManager.GetTranslations().GenericPhrases.ReasonField, reason);
            eb.AddField(TranslationManager.GetTranslations().GenericPhrases.AuthorField, $"<@{command.User.Id}>");
            await command.RespondAsync(embed: eb.Build());
            DiscordUserConfig cfg = DiscordManager.GetUserConfig(user.Id);
            UserWarning warning = new(command.User.Username, command.User.Id, reason);
            cfg.UserWarnings.AddWarning(warning);
        }

        public override void BuildOptions()
        {
            base.BuildOptions();
            CommandOptionsBase cob = new CommandOptionsBase()
            {
                Name = "user",
                Description = "User which to warn",
                OptionType = ApplicationCommandOptionType.User,
                Required = true,
            };
            CommandOptionsBase cob1 = new CommandOptionsBase()
            {
                Name = "reason",
                Description = "Reason for being warned",
                OptionType = ApplicationCommandOptionType.String,
                Required = true
            };
            CommandOptionsBase cob2 = new CommandOptionsBase()
            {
                Name = "senddm",
                Description = "Should the bot message the warned user?",
                OptionType = ApplicationCommandOptionType.Boolean,
                Required = true
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob1);
            Options.Add(cob2);
        }
    }
}
