using Discord;
using Discord.Rest;
using Discord.WebSocket;
using LurkbotV5.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5.Commands
{
    public class CommandLockdown : CommandBase
    {
        public override string CommandName => "lockdown";
        public override string Description => "Locks a channel down with an optional 30 second warning.";
        public override GuildPermission RequiredPermission => GuildPermission.Administrator;

        public async override void Execute(SocketSlashCommand command)
        { 

            bool showCountdown = false;
            string decontamvideo = "https://cdn.discordapp.com/attachments/887518399939350538/941157854734323712/SCP_SL_Light_Containment_Zone_Decontamination_30_Seconds.mp4";
            var channel = command.Channel;
            if (command.Data.Options.First().Value is not null)
            {
                if((bool)command.Data.Options.First().Value == true)
                {
                    showCountdown = true;
                }
            }
            if(channel is null)
            {
                Log.WriteError("Failed to get channel in lockdown command!");
                await command.RespondAsync("There was an error, see log.");
                return;
            }


            if(channel.GetChannelType() != ChannelType.Text) 
            {
                await command.RespondAsync("This command can only be used in text channels.");
                return;
            }

            var textchannel = (SocketTextChannel)channel;
            await command.RespondAsync(TranslationManager.GetTranslations().GenericPhrases.Acknowledged);
            if(!showCountdown)
            {
                if (!Configuration.ChannelLocks.ContainsKey(textchannel.Id))
                {
                    var perms = new OverwritePermissions();
                    perms.Modify(sendMessages: PermValue.Deny, sendMessagesInThreads: PermValue.Deny, createPrivateThreads: PermValue.Deny, createPublicThreads: PermValue.Deny, addReactions: PermValue.Deny);
                    var role = Bot.Instance.GetClient().GetGuild(textchannel.Guild.Id).EveryoneRole;
                    Configuration.ChannelLocks.Add(textchannel.Id, textchannel.PermissionOverwrites.ToList());
                    foreach (var thing in textchannel.PermissionOverwrites)
                    {
                        if (thing.TargetType == PermissionTarget.Role)
                        {
                            var r = Bot.Instance.GetClient().GetGuild(textchannel.Guild.Id).GetRole(thing.TargetId);
                            await textchannel.RemovePermissionOverwriteAsync(r);
                        }
                    }
                    await textchannel.AddPermissionOverwriteAsync(role, perms);
                    await textchannel.SendMessageAsync(TranslationManager.GetTranslations().LockdownPhrases.ChannelLockdownStarted);
                }
                else
                {
                    await textchannel.SendMessageAsync(TranslationManager.GetTranslations().LockdownPhrases.ChannelLockdownEnded);
                    foreach (var thing in Configuration.ChannelLocks[textchannel.Id])
                    {
                        if(thing.TargetType == PermissionTarget.Role)
                        {
                            var r = Bot.Instance.GetClient().GetGuild(textchannel.Guild.Id).GetRole(thing.TargetId);
                            await textchannel.AddPermissionOverwriteAsync(r, thing.Permissions);
                        }
                    }
                    Configuration.ChannelLocks.Remove(textchannel.Id);
                }
            }
            else
            {
                if (!Configuration.ChannelLocks.ContainsKey(textchannel.Id))
                {
                    await textchannel.SendMessageAsync(TranslationManager.GetTranslations().LockdownPhrases.ChannelDelayedLockdownIssued + "\n" + decontamvideo);
                    await Task.Run(() => DelayedLockdown(textchannel));
                }
                else
                {
                    await textchannel.SendMessageAsync(TranslationManager.GetTranslations().LockdownPhrases.ChannelLockdownEnded);
                    foreach (var thing in Configuration.ChannelLocks[textchannel.Id])
                    {
                        if (thing.TargetType == PermissionTarget.Role)
                        {
                            var r = Bot.Instance.GetClient().GetGuild(textchannel.Guild.Id).GetRole(thing.TargetId);
                            await textchannel.AddPermissionOverwriteAsync(r, thing.Permissions);
                        }
                    }
                    Configuration.ChannelLocks.Remove(textchannel.Id);
                }
            }
        }

        async Task DelayedLockdown(SocketTextChannel textchannel)
        {
            await Task.Delay(30 * 1000);
            var perms = new OverwritePermissions();
            perms.Modify(sendMessages: PermValue.Deny, sendMessagesInThreads: PermValue.Deny, createPrivateThreads: PermValue.Deny, createPublicThreads: PermValue.Deny, addReactions: PermValue.Deny);
            var role = Bot.Instance.GetClient().GetGuild(textchannel.Guild.Id).EveryoneRole;
            Configuration.ChannelLocks.Add(textchannel.Id, textchannel.PermissionOverwrites.ToList());
            foreach (var thing in textchannel.PermissionOverwrites)
            {
                if (thing.TargetType == PermissionTarget.Role)
                {
                    var r = Bot.Instance.GetClient().GetGuild(textchannel.Guild.Id).GetRole(thing.TargetId);
                    await textchannel.RemovePermissionOverwriteAsync(r);
                }
            }
            await textchannel.AddPermissionOverwriteAsync(role, perms);
            await textchannel.SendMessageAsync(TranslationManager.GetTranslations().LockdownPhrases.ChannelDelyedLockdownStarted);
        }

        public override void BuildOptions()
        {
            CommandOptionsBase cob = new()
            {
                Name = "showcountdown",
                Description = "sends the SCP:SL decontamination 30 second warning.",
                OptionType = ApplicationCommandOptionType.Boolean,
                Required = false
            };
            Options.Clear();
            Options.Add(cob);
        }
    }
}
