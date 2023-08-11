using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.EventListeners
{
    public static class UserHandler
    {
        public static Task OnUserBanned(SocketUser user, SocketGuild guild)
        {
            DiscordManager.DeleteUserConfig(user.Id, guild.Id);
            return Task.CompletedTask;
        }

        public static Task OnUserJoin(SocketGuildUser user)
        {
            ulong id = user.Id;
            DiscordUserConfig config = DiscordManager.GetUserConfig(id);
            foreach (uint level in DiscordManager.LevelRoles.RoleLevels.Keys)
            {
                foreach (RoleLevel role in DiscordManager.LevelRoles.RoleLevels[level])
                {
                    if (config.XPLevel < level)
                    {
                        continue;
                    }
                    else
                    {
                        if (role.Action == RoleLevelActions.REMOVE)
                        {
                            SocketGuild guild = Bot.Instance.GetClient().GetGuild(Bot.Instance.GetConfig().GuildID);
                            SocketRole grole = guild.GetRole(role.RoleID);
                            if (user != null && user.Roles.Contains(grole))
                            {
                                user.RemoveRoleAsync(grole);
                            }
                        }
                        else if (role.Action == RoleLevelActions.ADD)
                        {
                            SocketGuild guild = Bot.Instance.GetClient().GetGuild(Bot.Instance.GetConfig().GuildID);
                            SocketRole grole = guild.GetRole(role.RoleID);
                            if (user != null && grole != null && !user.Roles.Contains(grole))
                            {
                                user.AddRoleAsync(grole);
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
