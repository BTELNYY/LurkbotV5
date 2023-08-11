using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.EventListeners
{
    //TODO: Implement listener to move it out from DiscordManager
    public static class MessageListener
    {
        public static Task OnGhostPinging(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
        {
            Log.WriteDebug("OnGhostPing event triggered");
            if (msg.Value == null)
            {
                Log.WriteWarning("OnGhostPing msg is null!");
                return Task.CompletedTask;
            }
            if (msg.Value.Author.Id == Bot.Instance.GetClient().CurrentUser.Id)
            {
                return Task.CompletedTask;
            }
            if (DiscordManager.DoNotNotifyGhostPingCache.Contains(msg.Value.Id))
            {
                return Task.CompletedTask;
            }
            if (msg.Value.MentionedUserIds.Count > 0)
            {
                if (msg.Value.MentionedUserIds.Count == 1 && (msg.Value.MentionedUserIds.First() == msg.Value.Author.Id || msg.Value.MentionedUserIds.Contains(Bot.Instance.GetClient().CurrentUser.Id)))
                {
                    return Task.CompletedTask;
                }
                //has ghost pings
                string message = "";
                foreach (ulong id in msg.Value.MentionedUserIds)
                {
                    if (id == Bot.Instance.GetClient().CurrentUser.Id)
                    {
                        continue;
                    }
                    message += ("<@" + id.ToString() + ">");
                    message += " ";
                }
                Log.WriteDebug("Mentioned: " + msg.Value.MentionedUserIds.Count);
                EmbedBuilder eb = new();
                eb.WithTitle("uh oh, you're getting ghost pinged!");
                eb.WithCurrentTimestamp();
                eb.AddField("Content", msg.Value.Content);
                eb.AddField("Author", "<@" + msg.Value.Author.Id + ">");
                eb.Color = Color.Blue;
                channel.Value.SendMessageAsync(message, embed: eb.Build());
            }
            return Task.CompletedTask;
        }

        public static Task OnMessageDeleted(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
        {
            if (msg.Value == null)
            {
                Log.WriteWarning("OnMessageDeleted msg is null!");
                return Task.CompletedTask;
            }
            if (msg.Value.Author.Id == Bot.Instance.GetClient().CurrentUser.Id)
            {
                return Task.CompletedTask;
            }
            var channel1 = Bot.Instance.GetClient().GetChannel(Bot.Instance.GetConfig().DeletedMessagesChannelID) as ITextChannel;
            if (channel1 == null)
            {
                Log.WriteFatal("Channel not found! " + Bot.Instance.GetConfig().DeletedMessagesChannelID);
                return Task.CompletedTask;
            }

            EmbedBuilder eb = new();
            eb.WithTitle("Deleted Message");
            eb.AddField("Author", "<@" + msg.Value.Author.Id + ">");
            eb.AddField("Channel", "<#" + channel.Id + ">");
            if (msg.Value.Content != null || !string.IsNullOrEmpty(msg.Value.Content))
            {
                if (msg.Value.Content.Length > 0)
                {
                    eb.AddField("Content (text)", msg.Value.Content);
                }
            }
            if (msg.Value.Attachments.Count > 0)
            {
                string atturls = "";
                foreach (var att in msg.Value.Attachments)
                {
                    string attachmentparsed = att.Url.Replace("media", "cdn").Replace("net", "com");
                    Log.WriteDebug(attachmentparsed);
                    Log.WriteDebug(att.Url);
                    if (att == msg.Value.Attachments.Last())
                    {
                        atturls += attachmentparsed;
                    }
                    else
                    {
                        atturls += attachmentparsed + "\n";
                    }
                }
                eb.AddField("Attachments", atturls);
                Embed[] embeds = { eb.Build() };
                channel1.SendMessageAsync(embeds: embeds);
            }
            else
            {
                channel1.SendMessageAsync(embed: eb.Build());
            }
            return Task.CompletedTask;
        }

    }
}
