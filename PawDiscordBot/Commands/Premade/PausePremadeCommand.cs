using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands.Premade
{
    /// <summary>
    /// Functionality to Pause/Unpause the client
    /// </summary>
    internal class PausePremadeCommand : PremadeCommand
    {
        internal bool PauseValueToSet { get; set; }

        internal PausePremadeCommand(PremadeCommandType feat, string trigger, bool valueToSet) : base(feat, trigger)
        {
            this.PauseValueToSet = valueToSet;
        }

        public override bool HandleMessage(PawDiscordBotClient client, SocketUserMessage message)
        {
            client.PauseMessaging = this.PauseValueToSet;
            return true;
        }
    }
}
