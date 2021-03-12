using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands.Premade
{
    /// <summary>
    /// General-use class that contains an action to invoke when the message is handled.
    /// This makes it possible to make quick commands without having to create a new class.
    /// </summary>
    internal class BasicPremadeCommand : PremadeCommand
    {
        public Action<PawDiscordBotClient, SocketUserMessage> Action { get; set; }

        public BasicPremadeCommand(PremadeCommandType feat, string trigger, Action<PawDiscordBotClient, SocketUserMessage> act) : base(feat, trigger)
        {
            this.Action = act;
        }

        public override bool HandleMessage(PawDiscordBotClient client, SocketUserMessage message)
        {
            bool handled = false;
            if (Action != null)
                Action.Invoke(client, message);

            return handled;
        }
    }
}
