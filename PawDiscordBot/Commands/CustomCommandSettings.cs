using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    public class CustomCommandSettings
    {
        public int ParameterCount { get; set; }

        public string RequiredPrefix { get; set; }

        public CustomCommandSettings(string requiredPrefix, int parameterCount)
        {
            if (string.IsNullOrEmpty(requiredPrefix))
                throw new PawDiscordBotException("All commands must have a prefix!");

            this.ParameterCount = parameterCount;
            this.RequiredPrefix = requiredPrefix;
        }
    }
}
