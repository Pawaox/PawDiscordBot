using Discord.WebSocket;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    /// <summary>
    /// Used to convert user input with multiple sections (separated by spaces) into an action
    /// This command will handle basic parsing of user input into separated lists.
    /// Extend HandleCommand and add your logic there
    /// </summary>
    public abstract class ParameterCommand : PawDiscordCommandBase
    {
        public virtual CommandSettings Settings { get; set; }

        public Action<SocketUserMessage, string[], string[]> Action { get; set; }

        public ParameterCommand() { }
        public ParameterCommand(CommandSettings settings)
        {
            this.Settings = settings;
        }
        public ParameterCommand(CommandSettings settings, Action<SocketUserMessage, string[], string[]> act) : this(settings)
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

                    //Count backwards to get the parameters
                    for (int i = splt.Length - Settings.ParameterCount; i < splt.Length; i++)
                        parameters.Add(splt[i]);

                    //Remaining sections goes into levels
                    for (int i = 0; i < remaining + 1; i++)
                        levels.Add(splt[i]);
                }
                else
                    throw new PawDiscordBotException(ExceptionType.WARN_USER, "Not enough arguments! Expected" + minCount + ", got " + splt.Length);
            }

            return HandleCommand(message, levels.ToArray(), parameters.ToArray());
        }

        public abstract bool HandleCommand(SocketUserMessage msg, string[] levels, string[] parameters);
    }
}
