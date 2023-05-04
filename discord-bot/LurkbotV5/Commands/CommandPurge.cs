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
            await command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Acknowledged);
            var channel = command.Channel;
            var messages = channel.GetMessagesAsync((int) amount).FlattenAsync().Result.ToList();
            foreach(var message in messages)
            {
                await message.DeleteAsync();
            }
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
            Options.Clear();
            Options.Add(cob);
        }
    }
}
