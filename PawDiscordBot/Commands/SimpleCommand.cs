using Discord.WebSocket;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    public class SimpleCommand : PawDiscordCommandBase
    {
        public Action<SocketUserMessage> Action { get; set; }

        public SimpleCommand() { }
        public SimpleCommand(Action<SocketUserMessage> act) { this.Action = act; }

        public override bool HandleMessage(SocketUserMessage message)
        {
            Action?.Invoke(message);

            return true;
        }
    }
}
