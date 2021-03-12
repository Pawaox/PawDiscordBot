using CasualBot.Commands;
using Discord.WebSocket;
using PawDiscordBot;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualBot
{
    public class CasualBotClient : PawDiscordBotClient
    {
        public CasualBotClient(string key, string logName = "[CasualBot]") : base(key, logName)
        {
            CanReplyWithErrors = true;
            CanReplyWithExceptions = true;
            ReactOnlyIfMentionedFirst = false;
            CanReactToBotMessages = false;

            Commands.AddPremadeCommand(PremadeCommandType.PAUSE, "¤pause");
            Commands.AddPremadeCommand(PremadeCommandType.UNPAUSE, "¤unpause");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_PAWDISCORDBOT, "¤crash_a");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_NULLPOINTER, "¤crash_b");


            Commands.AddCommand("bangh", new CasualSimpleCommand(msg => msg.Channel.SendMessageAsync("bongh")));
            Commands.AddCommand("brown", new CasualSimpleCommand(msg => msg.Channel.SendMessageAsync("brown = down")));
            Commands.AddCommand("red", new CasualSimpleCommand(msg => msg.Channel.SendMessageAsync("red = ded")));


            Commands.AddCommand(new AddReactionToMessageCommand());
        }

        public override void ConnectionStarting()
        {
        }

        public override void ConnectionStarted()
        {
            Client.UserUpdated += Client_UserUpdated;

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

        private Task Client_UserUpdated(SocketUser arg1, SocketUser arg2)
        {
            return Task.Factory.StartNew(() =>
            {
                if (arg1 is SocketGuildUser && arg2 is SocketGuildUser)
                {
                    SocketGuildUser before = (SocketGuildUser)arg1;
                    SocketGuildUser after = (SocketGuildUser)arg2;

                    if (!before.Roles.Equals(after.Roles))
                    {
                        //Roles were changed
                        foreach (var role in before.Roles)
                        {
                            var Casual = after.Guild.GetRole(813345189048483860);
                            if (role.Name.Equals("Casual - Officer"))
                            {
                                after.AddRoleAsync(Casual);
                            }
                        }
                    }
                }
            });
        }
    }
}
