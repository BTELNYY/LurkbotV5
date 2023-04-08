using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace LurkbotV5.Commands
{
    public class CommandRank : CommandBase
    {
        public override string CommandName => "rank";
        public override string Description => "views your or another users rank";
        public override GuildPermission RequiredPermission => GuildPermission.UseApplicationCommands;

        public async override void Execute(SocketSlashCommand command)
        {
            ulong id;
            SocketUser user;
            if (command.Data.Options.Count == 0)
            {
                id = command.User.Id;
                user = command.User;
            }
            else
            {
                user = (SocketUser)command.Data.Options.ToList()[0].Value;
                id = user.Id;
            }
            DiscordUserConfig cfg = DiscordManager.GetUserConfig(id);
            DiscordManager.SetUserConfig(cfg);
            EmbedBuilder eb = new();
            eb.WithTitle("Rank data: " + user.Username);
            eb.AddField("Access Tier", cfg.XPLevel);
            eb.AddField("XP/RequiredXP", string.Format("{0:0.0}", cfg.XP) + "/" + DiscordManager.GetXPPerLevel(cfg.XPLevel));
            eb.AddField("XP Locked?", cfg.LockXP ? "Yes" : "No");
            eb.WithColor(Color.Blue);
            eb.WithCurrentTimestamp();
            try
            {
                await command.RespondAsync(embed: eb.Build());
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
            }
            return;

        }

        public override void BuildOptions()
        {
            Options.Clear();
            CommandOptionsBase cop = new()
            {
                Required = false,
                OptionType = ApplicationCommandOptionType.User,
                Name = "user",
                Description = "The Discord ID of the user you want to modify the XP of. (right click on user and copy ID)"
            };
            Options.Add(cop);
        }
    }
}
