using Discord;
using Discord.WebSocket;
using LurkbotV5.BaseClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.SlashCommands
{
    public class CommandResetWarnings : CommandBase
    {
        public override string CommandName => "resetwarnings";
        public override string Description => "Reset Warnings";
        public override GuildPermission RequiredPermission => GuildPermission.Administrator;
        public override void Execute(SocketSlashCommand command)
        {
            base.Execute(command);
            int counter = 0;
            int errorCounter = 0;
            foreach (string file in Directory.EnumerateFiles(DiscordManager.UserConfigPath))
            {
                try
                {
                    DiscordUserConfig config = JsonConvert.DeserializeObject<DiscordUserConfig>(File.ReadAllText(file));
                    config.UserWarnings = new();
                    DiscordManager.WriteUserConfig(config);
                }
                catch(Exception ex)
                {
                    Log.WriteError(ex.ToString());
                    errorCounter++;
                }
                counter++;
            }
            command.RespondAsync($"Done! Files Parsed: {counter}, Errors: {errorCounter}");
        }
    }
}
