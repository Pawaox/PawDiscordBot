using Discord.WebSocket;
using PawDiscordBot;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot
{
    public class GnomeParsingBotClient : PawDiscordBotClient
    {
        public GnomeParsingBotClient(string key, string logName = "[GnomeParsingBot]") : base(key, logName)
        {
            CanReplyWithErrors = true;
            CanReplyWithExceptions = true;
            ReactOnlyIfMentionedFirst = false;
            CanReactToBotMessages = false;

            AllowedServerChannelCombo = new Tuple<ulong, ulong>(701098779126005820, 816316160633995274);

            string prefix = "!¤";
            //Modules.RegisterPremadeModule(PawDiscordBot.Modules.PremadeModuleType.MUSIC);

            Commands.AddPremadeCommand(PremadeCommandType.PAUSE, prefix + "pause");
            Commands.AddPremadeCommand(PremadeCommandType.UNPAUSE, prefix + "unpause");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_PAWDISCORDBOT, prefix + "crash_a");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_NULLPOINTER, prefix + "crash_b");

            //Commands.AddCommand("createPost", new SimpleCommand(msg => msg.Channel.SendMessageAsync("Text", 
        }


        public override void ConnectionStarting()
        {
        }

        public override void ConnectionStarted()
        {
            //Client.UserUpdated += Client_UserUpdated;

            OnUnhandledMessage += (message) =>
            {
                try
                {
                    //Testing stuff, and making features for the first time, can be done here.
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                }
            };
        }
    }
}
