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
    public class CommandLeaderboard : CommandBase
    {
        public override string CommandName => "leaderboard";
        public override string Description => "get the play time leaderboard";
        public override GuildPermission RequiredPermission => base.RequiredPermission;
        public override bool IsDefaultEnabled => true;
        public override void Execute(SocketSlashCommand command)
        {
            try
            {
                PlayerStats[] players = APIManager.GetPlaytimeLeaderboard();
                EmbedBuilder eb = new();
                eb.WithColor(Color.Blue);
                eb.WithTitle(TranslationManager.GetTranslations().PlaytimeLeaderboard);
                string description = "``` (PLACE): (USERNAME), (PLAYTIME) \n";
                int counter = 1;
                foreach (var player in players)
                {
                    TimeSpan t = TimeSpan.FromSeconds(player.PlayTime);
                    string answer = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                                    t.Hours + (t.Days * 24),
                                    t.Minutes,
                                    t.Seconds);
                    description += $"{counter}: {player.LastNickname}, {answer} \n";
                    counter++;
                }
                description += "```";
                eb.WithCurrentTimestamp();
                eb.WithDescription(description);
                command.RespondAsync(embed: eb.Build());
            }
            catch (Exception ex)
            {
                command.Channel.SendMessageAsync("An error occured: \n " + ex.ToString());
                Log.WriteError("Error executing command. \n" + ex.ToString());
            }
        }
    }
}
