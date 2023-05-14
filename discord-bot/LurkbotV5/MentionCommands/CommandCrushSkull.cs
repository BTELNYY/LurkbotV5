using Discord;
using Discord.WebSocket;
using LurkbotV5.BaseClasses;
using LurkbotV5.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.MentionCommands
{
    public class CommandCrushSkull : MentionCommandBase
    {
        public override string Command => "crush his skull, thank you";
        public override GuildPermission Permission => GuildPermission.MuteMembers;

        public async override void Execute(MentionCommandParams param)
        {
            base.Execute(param);
            SocketMessage message = param.Message;
            if(message.Reference == null)
            {
                await message.Channel.SendMessageAsync(TranslationManager.GetTranslations().GenericErrorPhrases.MustReplyToMessage, messageReference: message.Reference);
                return;
            }
            if(message.Reference.MessageId.IsSpecified) 
            {
                var channel = Bot.Instance.GetClient().GetChannel(message.Reference.ChannelId) as ITextChannel;
                if(channel is null)
                {
                    Log.WriteError("Reply channel is null.");
                    await message.Channel.SendMessageAsync(TranslationManager.GetTranslations().MentionCommandPhrases.ErrorInCommand, messageReference: message.Reference);
                    return;
                }
                var target = channel.GetMessageAsync(message.Reference.MessageId.Value).Result.Author as SocketGuildUser;
                if(target is null)
                {
                    Log.WriteError("Target is null.");
                    await message.Channel.SendMessageAsync(TranslationManager.GetTranslations().MentionCommandPhrases.ErrorInCommand, messageReference: message.Reference);
                    return;
                }
                await target.SetTimeOutAsync(TimeSpan.FromSeconds(120));
                await message.Channel.SendMessageAsync(TranslationManager.GetTranslations().TimeoutPhrases.UserTimedOut.Replace("{user}", target.Username), messageReference: message.Reference);
            }
        }
    }
}
