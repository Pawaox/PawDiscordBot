using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    public class CommandSettings
    {
        /// <summary>
        /// The amount of expected parameters for this command
        /// </summary>
        public int ParameterCount { get; set; }

        /// <summary>
        /// If specified, associated command will only be triggered if userinput starts with this prefix
        /// </summary>
        public string RequiredPrefix { get; set; }

        public CommandSettings(string requiredPrefix, int parameterCount)
        {
            if (string.IsNullOrEmpty(requiredPrefix))
                throw new PawDiscordBotException("All commands must have a prefix!");

            this.ParameterCount = parameterCount;
            this.RequiredPrefix = requiredPrefix;
        }
    }
}
