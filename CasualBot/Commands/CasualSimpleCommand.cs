using Discord.WebSocket;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualBot.Commands
{
    public class CasualSimpleCommand : SimpleCommand
    {
        public CasualSimpleCommand(Action<SocketUserMessage> act) : base(act)
        {
        }
    }
}
