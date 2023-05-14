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
    public class CommandChannelMute : CommandBase
    {
        public override string CommandName => "channelmute";
        public override string Description => "Mute a user in this specific channel.";
        public override GuildPermission RequiredPermission => GuildPermission.MuteMembers;
        public async override void Execute(SocketSlashCommand command)
        {
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            SocketGuildUser user = (SocketGuildUser)options[0].Value;
            string reason = TranslationManager.GetTranslations().GenericPhrases.NoReason;
            if(command.Data.Options.Count > 1)
            {
                reason = (string)options[1].Value;
            }
            var channel = command.Channel;
            if (channel is null)
            {
                Log.WriteError("Failed to get channel in channelmute command!");
                await command.RespondAsync("There was an error, see log.");
                return;
            }
            var textchannel = (SocketTextChannel)channel;
            var perms = new OverwritePermissions();
            perms = perms.Modify(sendMessages: PermValue.Deny, sendMessagesInThreads: PermValue.Deny, createPrivateThreads: PermValue.Deny, createPublicThreads: PermValue.Deny, addReactions: PermValue.Deny);
            var userperm = textchannel.GetPermissionOverwrite(user);
            if(userperm is null)
            {
                await textchannel.AddPermissionOverwriteAsync(user, perms);
                EmbedBuilder eb = new();
                eb.WithTitle(TranslationManager.GetTranslations().ChannelMutePhrases.ChannelMuteUser.Replace("{user}", user.Username));
                eb.Color = Color.Blue;
                eb.AddField(TranslationManager.GetTranslations().GenericPhrases.AuthorField, $"<@{command.User.Id}>");
                eb.AddField(TranslationManager.GetTranslations().GenericPhrases.ReasonField, reason);
                await command.RespondAsync(embed: eb.Build());
                return;
            }
            else
            {
                await textchannel.AddPermissionOverwriteAsync(user, OverwritePermissions.InheritAll);
                await textchannel.RemovePermissionOverwriteAsync(user);
                EmbedBuilder eb = new();
                eb.WithTitle(TranslationManager.GetTranslations().ChannelMutePhrases.ChannelUnmuteUser.Replace("{user}", user.Username));
                eb.Color = Color.Blue;
                eb.AddField(TranslationManager.GetTranslations().GenericPhrases.AuthorField, $"<@{command.User.Id}>");
                eb.AddField(TranslationManager.GetTranslations().GenericPhrases.ReasonField, reason);
                await command.RespondAsync(embed: eb.Build());
                return;
            }

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
                Name = "reason",
                Description = "string reason",
                OptionType = ApplicationCommandOptionType.String,
                Required = false
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob2);
        }
    }
}
