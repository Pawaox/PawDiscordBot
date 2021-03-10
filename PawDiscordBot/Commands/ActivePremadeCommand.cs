using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    public class ActivePremadeCommand
    {
        public PremadeCommand Command { get; set; }
        public string Trigger { get; set; }
        public Action<SocketUserMessage> Implementation { get; set; }

        public ActivePremadeCommand() { }
        public ActivePremadeCommand(PremadeCommand feat, string trigger, Action<SocketUserMessage> implementation)
        {
            this.Command = feat;
            this.Trigger = Trigger;
            this.Implementation = implementation;
        }
    }
}
