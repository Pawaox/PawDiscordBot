using Discord.WebSocket;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    public abstract class ParameterCommand : PawDiscordCommandBase
    {
        public virtual CustomCommandSettings Settings { get; set; }

        public Action<SocketUserMessage, string[], string[]> Action { get; set; }

        public ParameterCommand() { }
        public ParameterCommand(CustomCommandSettings settings)
        {
            this.Settings = settings;
        }
        public ParameterCommand(CustomCommandSettings settings, Action<SocketUserMessage, string[], string[]> act) : this(settings)
        {
            this.Action = act;
        }

        public override bool HandleMessage(SocketUserMessage message)
        {
            string[] splt = message.Content.Split(' ');

            List<string> levels = new List<string>();
            List<string> parameters = new List<string>();

            if (Settings != null)
            {
                int minCount = Settings.ParameterCount + 1; //+1 since there has to be some kind of txt first.

                if (splt.Length >= Settings.ParameterCount + 1)
                {
                    int remaining = splt.Length - minCount;

                    for (int i = splt.Length - Settings.ParameterCount; i < splt.Length; i++)
                        parameters.Add(splt[i]);

                    for (int i = 0; i < remaining + 1; i++)
                        levels.Add(splt[i]);
                }
                else
                    throw new PawDiscordBotUserWarningException("Not enough arguments! Excepted" + minCount + ", got " + splt.Length);

            }

            return HandleCommand(message, levels.ToArray(), parameters.ToArray());
        }

        public abstract bool HandleCommand(SocketUserMessage msg, string[] levels, string[] parameters);
    }
}
