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
    internal abstract class PremadeCommand : PawDiscordCommandBase
    {
        /// <summary>
        /// The type of this command
        /// </summary>
        internal PremadeCommandType CommandType { get; set; }

        internal string Trigger { get; set; }

        internal PremadeCommand() { }
        internal PremadeCommand(PremadeCommandType type, string trig)
        {
            this.CommandType = type;
            this.Trigger = trig;
        }
    }
}
