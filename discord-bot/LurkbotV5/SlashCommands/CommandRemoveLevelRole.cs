using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LurkbotV5.Managers;
using LurkbotV5.BaseClasses;

namespace LurkbotV5.SlashCommands
{
    public class CommandRemoveLevelRole : CommandBase
    {
        public override string CommandName => "removelevelrole";
        public override string Description => "remove level roles";
        public override bool IsDefaultEnabled => true;
        public override GuildPermission RequiredPermission => GuildPermission.ManageRoles;
        public override async void Execute(SocketSlashCommand command)
        {
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            long level = (long)options[0].Value;
            ulong RoleID = ((SocketRole)options[1].Value).Id;
            if (!DiscordManager.RoleLevelExists((uint)level))
            {
                await command.RespondAsync(TranslationManager.GetTranslations().LevelRolePhrases.NoSuchRoleLevel);
                return;
            }
            else
            {
                try
                {
                    DiscordManager.RemoveLevelRole((uint)level, RoleID);
                    await command.RespondAsync(TranslationManager.GetTranslations().LevelRolePhrases.SuccessRemovingLevelRole);
                    return;
                }
                catch (Exception ex)
                {
                    await command.RespondAsync(TranslationManager.GetTranslations().LevelRolePhrases.GeneralFailure);
                    Log.WriteError("Executing Command " + command.CommandName + " threw an exception: \n" + ex.ToString());
                }
            }
        }
        public override void BuildOptions()
        {
            CommandOptionsBase cob = new()
            {
                Name = "level",
                Description = "level at which the role should be granted",
                OptionType = ApplicationCommandOptionType.Integer,
                Required = true
            };
            CommandOptionsBase cob2 = new()
            {
                Name = "role",
                Description = "Mention of the role",
                OptionType = ApplicationCommandOptionType.Role,
                Required = true
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob2);
        }
    }
}
