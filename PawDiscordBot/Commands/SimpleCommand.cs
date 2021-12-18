using Discord.WebSocket;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    /// <summary>
    /// Used to convert a simple textstring into an action.
    /// Can be used as-is or inherited from to extend functionality
    /// </summary>
    public class SimpleCommand : PawDiscordCommandBase
    {
        public Action<SocketUserMessage> Action { get; set; }

        public SimpleCommand() { }
        public SimpleCommand(Action<SocketUserMessage> act) { this.Action = act; }

        public override bool HandleMessage(PawDiscordBotClient client, SocketUserMessage message)
        {
            Action?.Invoke(message);

            return true;
        }
    }
}
