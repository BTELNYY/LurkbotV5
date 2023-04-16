using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LurkbotV5.Managers;

namespace LurkbotV5.Commands
{
    public class CommandAddLevelRole : CommandBase
    {
        public override string CommandName => "addlevelrole";
        public override string Description => "Set level roles";
        public override bool IsDefaultEnabled => true;
        public override GuildPermission RequiredPermission => GuildPermission.ManageRoles;
        public override async void Execute(SocketSlashCommand command)
        {
            long level = (long)command.Data.Options.ToList()[0].Value;
            ulong RoleID = ((SocketRole)command.Data.Options.ToList()[1].Value).Id;
            RoleLevelActions flag = RoleLevelActions.ADD;
            try
            {
                Log.WriteDebug("Parsing Tag: " + command.Data.Options.ToList()[2].Value.ToString());
                flag = Enum.Parse<RoleLevelActions>(command.Data.Options.ToList()[2].Value.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteError("Error occured while parsing tag: \n" + ex.ToString());
                await command.RespondAsync(TranslationManager.GetTranslations().LevelRolePhrases.GeneralFailure);
                return;
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
