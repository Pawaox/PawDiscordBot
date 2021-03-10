using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Exceptions
{
    public class PawDiscordBotUserWarningException : PawDiscordBotException
    {
        public PawDiscordBotUserWarningException(string message) : base(message)
        {

        }
    }
}
