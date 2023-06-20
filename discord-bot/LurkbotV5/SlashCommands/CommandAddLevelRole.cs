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
    public class CommandAddLevelRole : CommandBase
    {
        public override string CommandName => "addlevelrole";
        public override string Description => "Set level roles";
        public override bool IsDefaultEnabled => true;
        public override GuildPermission RequiredPermission => GuildPermission.ManageRoles;
        public override async void Execute(SocketSlashCommand command)
        {
            SocketSlashCommandDataOption[] options = GetOptionsOrdered(command.Data.Options.ToList());
            long level = (long)options[0].Value;    
            ulong RoleID = ((SocketRole)options[1].Value).Id;
            RoleLevelActions flag = RoleLevelActions.ADD;
            try
            {
                Log.WriteDebug("Parsing Tag: " + options[2].Value.ToString());
                flag = Enum.Parse<RoleLevelActions>((string) options[2].Value);
            }
            catch (Exception ex)
            {
                Log.WriteError("Error occured while parsing tag: \n" + ex.ToString());
                await command.RespondAsync(TranslationManager.GetTranslations().LevelRolePhrases.GeneralFailure);
            }

            DiscordManager.AddLevelRole((uint)level, RoleID, flag);
            try
            {
                await command.RespondAsync(TranslationManager.GetTranslations().LevelRolePhrases.SuccessAddingLevelRole);
            }
            catch (Exception ex)
            {
                Log.WriteError("Executing Command " + command.CommandName + " threw an exception: \n" + ex.ToString());
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
            CommandOptionsBase cob3 = new()
            {
                Name = "action",
                Description = "ADD or REMOVE",
                OptionType = ApplicationCommandOptionType.String,
                Required = true
            };
            Options.Clear();
            Options.Add(cob);
            Options.Add(cob2);
            Options.Add(cob3);
        }
    }
}
