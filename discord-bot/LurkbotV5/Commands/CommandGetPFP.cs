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
            
            if(user.GetGuildAvatarUrl(size: 512) != user.GetAvatarUrl(size: 512))
            {
                HasGuildPfp = true;
            }
            EmbedBuilder eb = new();
            eb.Title = user.Username + " (" + id + ")";
            eb.AddField(TranslationManager.GetTranslations().HasGuildPFP + "?", HasGuildPfp.ToString());
            eb.ImageUrl = user.GetAvatarUrl(size: 512);
            url = user.GetAvatarUrl();
            string guildurl = user.GetGuildAvatarUrl(size: 512);
            eb.AddField(TranslationManager.GetTranslations().PFPURL, url);
            if(HasGuildPfp && guildurl != null)
            {
                if(guildurl == string.Empty || guildurl == null)
                {
                    Log.WriteWarning("Guild URL is NULL!");
                }
                eb.AddField(TranslationManager.GetTranslations().GuildPFPURL, guildurl);
            }
            eb.WithCurrentTimestamp();
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
