using Discord;
using Discord.WebSocket;
using LurkbotV5.BaseClasses;
using LurkbotV5.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.SlashCommands
{
    public class CommandSetRank : CommandBase
    {
        public override string CommandName => "setrank";
        public override CommandType CommandType => CommandType.DISCORD;
        public override GuildPermission RequiredPermission => GuildPermission.Administrator;
        public override string Description => "Set a users rank.";

        public override void Execute(SocketSlashCommand command)
        {
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            SocketUser user;
            long level = 0;
            double xp = 0.0f;
            user = (SocketUser)options[0].Value;
            level = (long)options[1].Value;
            if (command.Data.Options.Count > 2)
            {
                xp = (double)options[2].Value;
            }
            DiscordUserConfig cfg = DiscordManager.GetUserConfig(user.Id);
            cfg.XP = (float)xp;
            cfg.XPLevel = (uint)level;
            DiscordManager.WriteUserConfig(cfg);
            command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Success);
        }

        public override void BuildOptions()
        {
            Options.Clear();
            CommandOptionsBase cop = new()
            {
                Required = true,
                OptionType = ApplicationCommandOptionType.User,
                Name = "user",
                Description = "The Discord ID of the user you want to modify the rank of. (right click on user and copy ID)"
            };
            CommandOptionsBase cop1 = new()
            {
                Required = true,
                OptionType = ApplicationCommandOptionType.Integer,
                Name = "level",
                Description = "The level to set"
            };
            CommandOptionsBase cop2 = new()
            {
                Required = false,
                OptionType = ApplicationCommandOptionType.Number,
                Name = "xpamount",
                Description = "XP Amount"
            };
            Options.Add(cop);
            Options.Add(cop1);
            Options.Add(cop2);
        }
    }
}
