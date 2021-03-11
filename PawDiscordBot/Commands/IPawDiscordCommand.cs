using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    /// <summary>
    /// Base class which all commands should inherit from
    /// </summary>
    public abstract class PawDiscordCommandBase
    {
        public abstract bool HandleMessage(SocketUserMessage message);
    }
}
