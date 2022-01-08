using Discord;
using Discord.Rest;
using Discord.WebSocket;
using PawDiscordBot;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.Commands
{
    public class FindPostCommand : ParameterCommand
    {
        public FindPostCommand(string prefix) : base(new CommandSettings(prefix + "find", 1)) { }

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

                string inputSearchLength = parameters[0];
                int msgToSearch = 20;

                if (int.TryParse(inputSearchLength, out int newCount))
                    msgToSearch = newCount;

                Task.Factory.StartNew(() =>
                {
                    bool didFindAndSet = ActivePost.FindAndSetPostID(client, msg.Channel, msgToSearch).Result;

                    if (!didFindAndSet)
                        msg.Channel.SendMessageAsync("Couldn't find a post :(");
                });

                handled = true;
            }

            msg.DeleteAsync();
            return handled;
        }
    }
}
