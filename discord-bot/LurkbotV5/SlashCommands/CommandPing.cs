using Discord;
using Discord.WebSocket;
using LurkbotV5.BaseClasses;

namespace LurkbotV5.SlashCommands;
internal class CommandPing : CommandBase
{
    public override string CommandName => "ping";
    public override string Description => "Pong!";
    public override bool IsDefaultEnabled => true;
    public override GuildPermission RequiredPermission => GuildPermission.UseApplicationCommands;
    public override async void Execute(SocketSlashCommand command)
    {
        await command.RespondAsync("Pong!" + " Current ping is " + Bot.Instance.GetClient().Latency + "ms");
    }
}
