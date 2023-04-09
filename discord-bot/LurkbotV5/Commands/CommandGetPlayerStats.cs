using btelnyy.ConfigLoader.API;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LurkbotV5;
using LurkbotV5.Managers;

namespace LurkbotV5.Commands;
internal class CommandGetPlayerStats : CommandBase
{
    public override string CommandName => "getplayerdetails";
    public override string Description => "Get details on a player";
    public override bool IsDefaultEnabled => true;
    public override GuildPermission RequiredPermission => GuildPermission.UseApplicationCommands;
    public override CommandType CommandType => CommandType.SL;
    public override async void Execute(SocketSlashCommand command)
    {
        Embed[] embeds;
        PlayerStats stats = APIManager.GetPlayerStats(command.Data.Options.First().Value.ToString());
        if (stats.PlayTime == -1)
        {
            await command.RespondAsync(embed: Utility.GetErrorEmbed("Error", "Error fetching user."));
            return;
        }
        EmbedBuilder eb = new();
        eb.WithColor(Color.Blue);
        eb.WithTitle(TranslationManager.GetTranslations().PlayerDetails + stats.LastNickname);
        eb.AddField(TranslationManager.GetTranslations().SteamID, stats.SteamID);
        eb.AddField(TranslationManager.GetTranslations().FirstSeen, stats.FirstSeen.ToString("dd-MM-yyyy") + " at " + stats.FirstSeen.ToString("hh\\:mm\\:ss"));
        eb.AddField(TranslationManager.GetTranslations().LastSeen, stats.LastSeen.ToString("dd-MM-yyyy") + " at " + stats.LastSeen.ToString("hh\\:mm\\:ss"));
        if (stats.Usernames.Count > 0)
        {
            string NameStr = "```";
            NameStr += string.Join("\n", stats.Usernames);
            NameStr += "```";
            eb.AddField($"{TranslationManager.GetTranslations().OldNames} ({stats.Usernames.Count})", NameStr);
        }
        if (stats.PFlags.Count > 0)
        {
            string FlagsStr = "```";
            FlagsStr += "(ID), (FLAG), ISSUER, (COMMENTS), (ISSUE TIME) \n";
            foreach (Flags f in stats.PFlags)
            {

                FlagsStr += stats.PFlags.IndexOf(f) + ", ";
                FlagsStr += f.Flag.ToString() + ", ";
                FlagsStr += f.Issuer + ", ";
                FlagsStr += f.Comment + ", ";
                FlagsStr += f.IssueTime.ToString("dd-MM-yyyy") + " at " + f.IssueTime.ToString("hh\\:mm\\:ss") + "\n";
            }
            FlagsStr += "```";
            eb.AddField($"{TranslationManager.GetTranslations().Flags} ({stats.PFlags.Count})", FlagsStr);
        }
        TimeSpan t = TimeSpan.FromSeconds(stats.PlayTime);
        string answer = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                        t.Hours + (t.Days * 24),
                        t.Minutes,
                        t.Seconds);
        eb.AddField(TranslationManager.GetTranslations().PlayTime, answer);
        if (stats.LoginAmount != 0)
        {
            eb.AddField(TranslationManager.GetTranslations().Logins, stats.LoginAmount.ToString());
        }
        if (stats.TimeOnline != 0)
        {
            TimeSpan t1 = TimeSpan.FromSeconds(stats.TimeOnline);
            string answer1 = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                            t1.Hours + (t1.Days * 24),
                            t1.Minutes,
                            t1.Seconds);
            eb.AddField(TranslationManager.GetTranslations().TimeOnline, answer1);
        }
        eb.WithCurrentTimestamp();
        Embed embed = eb.Build();
        embeds = new Embed[1] { embed };
        try
        {
            await command.RespondAsync("", embeds);
        }
        catch (Exception ex)
        {
            await command.RespondAsync("An error occured! \n ```" + ex.ToString() + "```");
        }
    }
    public override void BuildOptions()
    {
        CommandOptionsBase cob = new()
        {
            Name = "user",
            Description = "name of online user or ID64 of user.",
            OptionType = ApplicationCommandOptionType.String,
            Required = true
        };
        Options.Clear();
        Options.Add(cob);
    }
}
