using PawDiscordBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualBot
{
    public class CasualBotClient : PawDiscordBotClient
    {
        public CasualBotClient(string key, string logName = "[CasualBot]") : base(key, logName)
        {
        }
    }
}
