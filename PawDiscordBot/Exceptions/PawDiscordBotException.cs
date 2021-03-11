using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Exceptions
{
    /// <summary>
    /// Extends from Exception and has an enum to help the catcher decide how to handle it.
    /// </summary>
    public class PawDiscordBotException : Exception
    {
        public ExceptionType ExceptionType { get; private set; }

        public PawDiscordBotException(ExceptionType type, string message) : base(message)
        {
            this.ExceptionType = type;
        }

        public PawDiscordBotException(string message) : this(ExceptionType.BASE, message) { }
    }
}
