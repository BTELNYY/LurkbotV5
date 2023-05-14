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
    public class CommandKickUser : CommandBase
    {
        public override string CommandName => "kickuser";
        public override string Description => "kick a user from this server.";
        public override GuildPermission RequiredPermission => GuildPermission.KickMembers;
        public override async void Execute(SocketSlashCommand command)
        {
            base.Execute(command);
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            SocketGuildUser user = (SocketGuildUser) options[0].Value;
            string reason = (string)options[1].Value;
            bool senddm = (bool)options[2].Value;
            bool hideauthor = (bool)options[3].Value;
            await user.KickAsync(reason);
            try
            {
                if (senddm)
                {
                    if (hideauthor && command.GuildId != null)
                    {
                        await user.SendMessageAsync(TranslationManager.GetTranslations().DirectMessagePhrases.UserKickedFromGuildNoAuthor.Replace("{server}", Bot.Instance.GetClient().GetGuild((ulong)command.GuildId).Name).Replace("{reason}", reason));
                    }
                    else
                    {
                    if (command.GuildId == null)
                    {
                        return;
                    }
                    await user.SendMessageAsync(TranslationManager.GetTranslations().DirectMessagePhrases.UserKickedFromGuildNoAuthor.Replace("{server}", Bot.Instance.GetClient().GetGuild((ulong)command.GuildId).Name).Replace("{reason}", reason).Replace("{author}", command.User.Username + "#" + command.User.Discriminator));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
            }
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithCurrentTimestamp();
            eb.WithTitle(TranslationManager.GetTranslations().ModerationPhrases.UserKicked.Replace("{user}", user.Username + "#" + user.Discriminator));
            eb.AddField(TranslationManager.GetTranslations().GenericPhrases.ReasonField, reason);
            if (!hideauthor)
            {
                eb.AddField(TranslationManager.GetTranslations().GenericPhrases.AuthorField, $"<@{command.User.Id}>");
            }
            if(hideauthor)
            {
                await command.RespondAsync(embed: eb.Build());
            }
            else
            {
                await command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Acknowledged, ephemeral: true);
                await command.Channel.SendMessageAsync(embed: eb.Build());
            }
        }

        public override void BuildOptions()
        {
            base.BuildOptions();
            CommandOptionsBase cob = new CommandOptionsBase()
            {
                Name = "user",
                Description = "User which to kick",
                OptionType = ApplicationCommandOptionType.User,
                Required = true,
            };
            CommandOptionsBase cob1 = new CommandOptionsBase()
            {
                Name = "reason",
                Description = "Reason for being kicked",
                OptionType = ApplicationCommandOptionType.String,
                Required = true
            };
            CommandOptionsBase cob2 = new CommandOptionsBase()
            {
                Name = "senddm",
                Description = "Should the bot message the kicked user?",
                OptionType = ApplicationCommandOptionType.Boolean,
                Required = true
            };
            CommandOptionsBase cob3 = new CommandOptionsBase() 
            {
                Name = "hideauthor",
                Description = "Should the bot hide that you kicked this user?",
                OptionType = ApplicationCommandOptionType.Boolean,
                Required = true
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob1);
            Options.Add(cob2);
            Options.Add(cob3);
        }
    }
}
