using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GnomeParsingBot.WarcraftLogs;
using PawDiscordBot;
using PawDiscordBot.Commands;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GnomeParsingBot.ActivePost;

namespace GnomeParsingBot.Commands
{
    public class AddRaidLogCommand : ParameterCommand
    {
        public AddRaidLogCommand(string prefix) : base(new CommandSettings(prefix + "add", 2)) { }

        public override bool HandleCommand(PawDiscordBotClient client, SocketUserMessage msg, string[] levels, string[] parameters)
        {
            bool handled = false;

            if (levels.Length != 1)
                return handled;

            if (Settings.ParameterCount != parameters.Length)
                return handled;

            if (msg.Author is SocketGuildUser)
            {
                SocketGuildUser socketUser = (SocketGuildUser)msg.Author;

                if (ActivePost.PostID.HasValue)
                {
                    LoggedRaid r = ActivePost.GetRaidFromLine(parameters[0]);
                    if (r != LoggedRaid.NONE)
                    {
                        string logID = parameters[1];

                        ActivePost.AddLog(r, new LoggedRaidData(r, logID));
                        ActivePost.RewriteMessage("", client, msg.Channel);
                    }

                    handled = true;
                }
                else
                    throw new PawDiscordBotException(ExceptionType.WARN_USER, "Please create a new post first!");
            }

            msg.DeleteAsync();
            return handled;
        }
    }
}
