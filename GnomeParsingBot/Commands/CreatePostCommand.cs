﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;
using PawDiscordBot;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.Commands
{
    public class CreatePostCommand : ParameterCommand
    {
        public CreatePostCommand(string prefix) : base(new CommandSettings(prefix + "post", 2)) { }

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
                string dateText = "";

                bool auto = "AUTO".Equals(parameters[1]?.ToUpper()?.Trim() ?? "");
                string dateInput = parameters[0]?.ToUpper() ?? "";

                DateTime autoDate = DateTime.MinValue;
                if (auto)
                {
                    if (DateTime.TryParse(dateInput, out autoDate))
                    {
                        dateText = autoDate.ToString("MMMM dd, yyyy");

                    }
                }
                else
                {
                    switch (parameters[0]?.ToUpper() ?? "")
                    {
                        case "":
                        case "NOW":
                        case "TODAY":
                            dateText = DateTime.Now.ToString("MMMM dd, yyyy");
                            break;
                    }
                }

                if (string.IsNullOrEmpty(dateText))
                {
                    dateText = dateInput;
                }

                string line = "Logs for " + dateText + Environment.NewLine;
                RestUserMessage reply = msg.Channel.SendMessageAsync(line).Result;

                if (auto)
                {
                    ActivePost.FetchFromWarcraftlogs(reply.Id, line, autoDate);
                    ActivePost.RewriteMessage(client, msg.Channel);
                }
                else
                {
                    ActivePost.ResetMessage(reply.Id, line);
                }

                handled = true;
            }

            msg.DeleteAsync();
            return handled;
        }
    }
}
