using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace LurkbotV5.BaseClasses
{
    public class MentionCommandBase
    {
        public virtual string Command { get; private set; } = "test";
        public virtual GuildPermission Permission { get; private set; } = GuildPermission.UseApplicationCommands;
        
        public virtual void Execute(MentionCommandParams param) 
        {
            
        }
    }

    public struct MentionCommandParams
    {
        public SocketGuildUser User { get; private set; }
        public ISocketMessageChannel Channel { get; private set; }
        public SocketMessage Message { get; private set; }
        public SocketGuild Guild { get; private set; }
        public string MessageContent { get; private set; }

        public MentionCommandParams(SocketGuildUser user, ISocketMessageChannel channel, SocketMessage message, SocketGuild guild, string messagecontent)
        {
            User = user;
            Channel = channel;
            Message = message;
            Guild = guild;
            MessageContent = messagecontent;
        }
    }
}
