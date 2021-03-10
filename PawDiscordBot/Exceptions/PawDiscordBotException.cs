using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Exceptions
{
    public class PawDiscordBotException : Exception
    {
        public PawDiscordBotException(string message) : base(message)
        {

        }
    }
}
