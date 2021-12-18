using Discord;
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
        public CreatePostCommand() : base(new CommandSettings("createPost", 1)) { }

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

            }

            handled = true;
            return handled;
        }
    }
}
