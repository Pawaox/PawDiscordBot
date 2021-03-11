using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    /// <summary>
    /// 'Wrap-class' combining the information needed for premade commands
    /// </summary>
    public class PremadeCommand
    {
        /// <summary>
        /// The type of this command
        /// </summary>
        public PremadeCommandType Command { get; set; }
        public string Trigger { get; set; }
        /// <summary>
        /// Action containing the implementation of this PremadeCommandType
        /// </summary>
        public Action<SocketUserMessage> Implementation { get; set; }

        public PremadeCommand() { }
        public PremadeCommand(PremadeCommandType feat, string trigger, Action<SocketUserMessage> implementation)
        {
            this.Command = feat;
            this.Trigger = Trigger;
            this.Implementation = implementation;
        }
    }
}
