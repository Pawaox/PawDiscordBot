using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    /// <summary>
    /// Available pre-programmed commands
    /// </summary>
    public enum PremadeCommandType
    {
        NONE,
        PAUSE,
        UNPAUSE,

        PURGE,

        TEST_EXCEPTION_PAWDISCORDBOT,
        TEST_EXCEPTION_NULLPOINTER
    }
}
