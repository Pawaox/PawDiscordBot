using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands.Premade
{
    /// <summary>
    /// 'Wrap-class' combining the information needed for premade commands
    /// </summary>
    public abstract class PremadeCommand : PawDiscordCommandBase
    {
        /// <summary>
        /// The type of this command
        /// </summary>
        public PremadeCommandType CommandType { get; set; }

        public string Trigger { get; set; }

        public PremadeCommand() { }
        public PremadeCommand(PremadeCommandType type, string trig)
        {
            this.CommandType = type;
            this.Trigger = trig;
        }
    }
}
