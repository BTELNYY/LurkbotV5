using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Commands
{
    public class CommandGetPFP : CommandBase
    {
        public override string CommandName => "userpfp";
        public override string Description => "Gets a users profile picture";
        public override GuildPermission RequiredPermission => GuildPermission.UseApplicationCommands;
        public override CommandType CommandType => CommandType.DISCORD;
        public override List<CommandOptionsBase> Options => base.Options;

        public override void Execute(SocketSlashCommand command)
        {
            ulong id;
            SocketGuildUser user;
            if (command.Data.Options.Count == 0)
            {
                id = command.User.Id;
                user = null;
            }
            else
            {
                user = (SocketGuildUser)command.Data.Options.ToList()[0].Value;
                id = user.Id;
            }
            string url = "";
            bool HasGuildPfp = false;
            if(user.GetGuildAvatarUrl(size: 512) == user.GetAvatarUrl(size: 512))
            {
                url = user.GetAvatarUrl(size: 512);
            }
            else
            {
                url = user.GetGuildAvatarUrl(size: 512);
                HasGuildPfp = true;
            }
            EmbedBuilder eb = new();
            eb.Title = user.Username + " (" + id + ")";
            eb.AddField("Has Guild PFP?", HasGuildPfp.ToString());
            eb.ImageUrl = url;
            eb.WithCurrentTimestamp();
            eb.WithUrl(url);
            Embed embed = eb.Build();
            command.RespondAsync(embed:embed);
        }

        public override void BuildOptions()
        {
            base.BuildOptions();
            CommandOptionsBase cob = new CommandOptionsBase()
            {
                Name = "user",
                Description = "mention the user which you want the pfp from",
                Required = true,
                OptionType = ApplicationCommandOptionType.User
            };
            Options.Clear();
            Options.Add(cob);
        }
    }
}
