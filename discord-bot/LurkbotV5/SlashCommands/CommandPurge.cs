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
    public class CommandPurge : CommandBase
    {
        public override string CommandName => "purge";
        public override string Description => "Delete a set amount of messages in this channel.";
        public override GuildPermission RequiredPermission => GuildPermission.ManageMessages;

        public override async void Execute(SocketSlashCommand command)
        {
            base.Execute(command);
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            long amount = (long)options[0].Value;
            bool deleteself = false;
            if(command.Data.Options.Count > 1) 
            {
                deleteself = (bool)options[1].Value;
            }
            await command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Acknowledged);
            var channel = command.Channel;
            //basically, because we want to specifically delete x amount of messages, and our bot message counts, if we dont delete it, add one extra so we delete x amount of messages
            int extraamount = 1;
            if(!deleteself) { extraamount = 0; }
            var messages = channel.GetMessagesAsync((int) amount + extraamount).FlattenAsync().Result.ToList();
            foreach (var message in messages)
            {
                if (message.Author.Id == Bot.Instance.GetClient().CurrentUser.Id && !deleteself)
                {
                    continue;
                }
                if (message.MentionedUserIds.Count > 0)
                {
                    DiscordManager.DoNotNotifyGhostPingCache.Add(message.Id);
                }
                await message.DeleteAsync();
            }
            DiscordManager.DoNotNotifyGhostPingCache.Clear();
        }

        public override void BuildOptions()
        {
            base.BuildOptions();
            CommandOptionsBase cob = new()
            {
                Name = "amount",
                Description = "How many messages to delete?",
                OptionType = ApplicationCommandOptionType.Integer,
                Required = true
            };
            CommandOptionsBase cob1 = new()
            {
                Name = "deleteself",
                Description = "Should the bot delete its own messages?",
                OptionType = ApplicationCommandOptionType.Boolean,
                Required = false
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob1);
        }
    }
}
