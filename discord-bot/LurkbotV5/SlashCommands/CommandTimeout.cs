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
    public class CommandTimeout : CommandBase   
    {
        public override string CommandName => "timeoutuser";
        public override string Description => "time a user out for a set amount of seconds.";
        public override GuildPermission RequiredPermission => GuildPermission.MuteMembers;

        public async override void Execute(SocketSlashCommand command)
        {
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            SocketGuildUser target = (SocketGuildUser) options[0].Value;
            long length = (long) options[1].Value;
            string reason = TranslationManager.GetTranslations().GenericPhrases.NoReason;
            if (command.Data.Options.Count > 2)
            {
                reason = (string)options[2].Value;
            }
            if(length < 30)
            {
                //too short of length
                await command.RespondAsync(TranslationManager.GetTranslations().TimeoutPhrases.DurationTooShort);
                return;
            }
            await target.SetTimeOutAsync(TimeSpan.FromSeconds(length));
            EmbedBuilder eb = new();
            eb.WithTitle(TranslationManager.GetTranslations().TimeoutPhrases.UserTimedOut.Replace("{user}", target.Username));
            eb.AddField(TranslationManager.GetTranslations().GenericPhrases.ReasonField, reason);
            eb.AddField(TranslationManager.GetTranslations().GenericPhrases.DurationField, length.ToString());
            eb.AddField(TranslationManager.GetTranslations().GenericPhrases.AuthorField, $"<@{command.User.Id}>");
            eb.WithColor(Color.Blue);
            await command.RespondAsync(embed: eb.Build());
        }

        public override void BuildOptions()
        {
            CommandOptionsBase cob = new()
            {
                Name = "user",
                Description = "user to mute",
                OptionType = ApplicationCommandOptionType.User,
                Required = true
            };
            CommandOptionsBase cob2 = new()
            {
                Name = "length",
                Description = "amount of seconds to mute them for",
                OptionType = ApplicationCommandOptionType.Integer,
                Required = true
            };
            CommandOptionsBase cob3 = new()
            {
                Name = "reason",
                Description = "string reason",
                OptionType = ApplicationCommandOptionType.String,
                Required = false
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob2);
            Options.Add(cob3);
        }
    }
}
