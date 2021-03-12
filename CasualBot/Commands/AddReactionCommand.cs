using Discord;
using Discord.WebSocket;
using PawDiscordBot;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualBot.Commands
{
    public class AddReactionToMessageCommand : ParameterCommand
    {
        public AddReactionToMessageCommand() : base(new CommandSettings("addReaction", 2)) { }

        public override bool HandleCommand(PawDiscordBotClient client, SocketUserMessage msg, string[] levels, string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
