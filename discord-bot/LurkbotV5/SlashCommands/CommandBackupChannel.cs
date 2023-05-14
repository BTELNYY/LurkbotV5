using Discord;
using Discord.WebSocket;
using LurkbotV5.BaseClasses;
using LurkbotV5.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace LurkbotV5.SlashCommands
{
    public class CommandBackupChannel : CommandBase
    {
        public override string CommandName => "backupmessages";
        public override string Description => "Creates a backup of every message every sent in this channel.";
        public override GuildPermission RequiredPermission => GuildPermission.Administrator;
        public async override void Execute(SocketSlashCommand command)
        {
            await command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Acknowledged);
            var channel = command.Channel;
            var messages = channel.GetMessagesAsync(int.MaxValue).FlattenAsync().Result;
            string filepath = $"./channel_backups/backup_{channel.Name}_{DateTime.Now.ToString("dd-MM-yyyy")}-{DateTime.Now.ToString("hh\\:mm\\:ss")}.txt";
            StreamWriter sw = new StreamWriter(filepath, append: true);
            ulong counter = 0;
            foreach (var message in messages)
            {
                string loggedstring = "";
                string date = message.Timestamp.ToString("dd-MM-yyyy");
                string time = message.Timestamp.ToString("hh\\:mm\\:ss");
                loggedstring += $"[{date}, {time}] [{message.Author.Username}#{message.Author.Discriminator} ({message.Author.Id})]: ";
                loggedstring += $"{message.Content}";
                foreach(var attachment in message.Attachments)
                {
                    loggedstring += $" {attachment.Url}";
                }
                loggedstring += "\n";
                counter++;
                Log.WriteDebug("Parsed " + counter + " messages for backup.");
                sw.Write(loggedstring);
            }
            sw.Close();
            await channel.SendMessageAsync(TranslationManager.GetTranslations().GenericPhrases.Success);
        }
    }
}
