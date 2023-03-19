using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using btelnyy.ConfigLoader.API;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.Net;
using LurkbotV5.Commands;
using System.Diagnostics.Metrics;

namespace LurkbotV5
{
    //class handles all discord specific stuff, such as leveling and commands
    public class DiscordManager
    {
        public Bot? Bot { get; private set; }
        public DiscordSocketClient Client { get; private set; }
        
        public Dictionary<string, CommandBase> Commands { get; private set; } = new Dictionary<string, CommandBase>();

        public DiscordManager(Bot bot, DiscordSocketClient client) 
        {
            Bot = bot;
            Client = client;
        }

        private void EventInit()
        {

        }

        public void BuildCommand(CommandBase command)
        {
            DiscordSocketClient client = Client;
            SlashCommandBuilder scb = new();
            if (Bot.Config.DisableNonSLCommands && command.CommandType != CommandType.SL)
            {
                return;
            }
            scb.WithName(command.CommandName);
            scb.WithDescription(command.Description);
            scb.WithDMPermission(command.IsDMEnabled);
            command.BuildOptions();
            foreach (CommandOptionsBase cop in command.Options)
            {
                Log.WriteDebug("Building option: " + cop.Name);
                scb.AddOption(cop.Name, cop.OptionType, cop.Description, cop.Required);
            }
            scb.DefaultMemberPermissions = command.RequiredPermission;
            scb.IsDefaultPermission = command.IsDefaultEnabled;
            command.BuildAliases();
            Commands.Add(command.CommandName, command);
            Log.WriteInfo("Registering Aliases for: " + command.CommandName + "; Alias: " + string.Join(", ", command.Aliases.ToArray()));
            foreach (string alias in command.Aliases)
            {
                Commands.Add(alias, command);
            }
            try
            {
                Log.WriteInfo("Building Command: " + command.CommandName);
                if (command.PrivateCommand)
                {
                    client.GetGuild(command.PrivateServerID).CreateApplicationCommandAsync(scb.Build());
                }
                else
                {
                    client.CreateGlobalApplicationCommandAsync(scb.Build());
                }
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Log.WriteError(json);
            }
            catch (Exception exception)
            {
                Log.WriteError("Failed to build command: " + command.CommandName + "\n Error: \n " + exception.ToString());
            }
        }
    }
}