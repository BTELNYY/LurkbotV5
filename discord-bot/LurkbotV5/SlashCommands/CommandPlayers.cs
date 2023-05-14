using Discord;
using Discord.WebSocket;
using LurkbotV5.BaseClasses;
using LurkbotV5.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.SlashCommands;
internal class CommandPlayers : CommandBase
{
    public override string CommandName => "playercount";
    public override string Description => "Get how many players are on the SL server.";
    public override bool IsDefaultEnabled => true;
    public override GuildPermission RequiredPermission => GuildPermission.UseApplicationCommands;
    public override CommandType CommandType => CommandType.SL;
    public override async void Execute(SocketSlashCommand command)
    {
        EmbedBuilder eb = new();
        eb.WithTitle("Player Count");
        eb.Color = Color.Blue;
        eb.WithCurrentTimestamp();
        foreach(ServerResponse resp in APIManager.GetServerStatus(Bot.Instance.GetConfig().AuthKey).value)
        {
            foreach(Server s in resp.Servers)
            {
                string name = "";
                if (Bot.Instance.GetConfig().ServerNames.ContainsKey(s.ID.ToString()))
                {
                    name = Bot.Instance.GetConfig().ServerNames[s.ID.ToString()];
                }
                else
                {
                    name = "[Missing Server Name]";
                }
                eb.AddField(name, s.PlayersList.Length);
            }
        }
        await command.RespondAsync(embed: eb.Build());
    }
}
