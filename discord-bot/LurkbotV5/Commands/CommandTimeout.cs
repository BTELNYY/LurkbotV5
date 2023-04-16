using Discord;
using Discord.WebSocket;
using LurkbotV5.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Commands
{
    public class CommandTimeout : CommandBase   
    {
        public override string CommandName => "timeoutuser";
        public override string Description => "time a user out for a set amount of seconds.";
        public override GuildPermission RequiredPermission => GuildPermission.MuteMembers;

        public async override void Execute(SocketSlashCommand command)
        {
            SocketGuildUser target = (SocketGuildUser) command.Data.Options.ToList()[0].Value;
            long length = (long) command.Data.Options.ToList()[1].Value;
            string reason = "No reason given.";
            if(command.Data.Options.Count > 2)
            {
                reason = (string) command.Data.Options.ToList()[2].Value;
            }
            if(length <= 30)
            {
                //too short of length
                await command.RespondAsync(TranslationManager.GetTranslations().TimeoutPhrases.DurationTooShort);
                return;
            }
            await target.SetTimeOutAsync(TimeSpan.FromSeconds(length));
            EmbedBuilder eb = new();
            eb.WithTitle(TranslationManager.GetTranslations().TimeoutPhrases.UserTimedOut.Replace("{user}", target.Username));
            eb.AddField(TranslationManager.GetTranslations().TimeoutPhrases.ReasonField, reason);
            eb.AddField(TranslationManager.GetTranslations().TimeoutPhrases.DurationField, length.ToString());
            eb.AddField(TranslationManager.GetTranslations().TimeoutPhrases.AuthorField, $"<@{command.User.Id}>");
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
