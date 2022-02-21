using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GnomeParsingBot.Commands;
using GnomeParsingBot.WarcraftLogs;
using PawDiscordBot;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GnomeParsingBot.ActivePost;

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

            //AllowedServerChannelCombo = new Tuple<ulong, ulong>(701098779126005820, 816316160633995274); //devspam
            AllowedServerChannelCombo = new Tuple<ulong, ulong>(701098779126005820, 853690415580577852); //C-LOGS
            //AllowedServerChannelCombo = new Tuple<ulong, ulong>(701098779126005820, 927306244010692629); //25m-pug-logs
            //AllowedServerChannelCombo = new Tuple<ulong, ulong>(701098779126005820, 933109362677911573); //10m-pug-logs


            UserWhiteList.Add(136534046502158336); //Pawa
            //UserWhiteList.Add(329275162874740737); //Consita

            string prefix = "!_";

            Commands.AddPremadeCommand(PremadeCommandType.PURGE, prefix + "purge");
            Commands.AddPremadeCommand(PremadeCommandType.PAUSE, prefix + "pause");
            Commands.AddPremadeCommand(PremadeCommandType.UNPAUSE, prefix + "unpause");
            Commands.AddPremadeCommand(PremadeCommandType.PURGE, prefix + "purge");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_PAWDISCORDBOT, prefix + "crash_a");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_NULLPOINTER, prefix + "crash_b");

            Commands.AddCommand(new FindPostCommand(prefix));
            Commands.AddCommand(new NewPostCommand(prefix));
            Commands.AddCommand(new AddRaidLogCommand(prefix));
            Commands.AddCommand(new GenerateSheetCommand(prefix));
            Commands.AddCommand(new GenerateAwardsCommand(prefix));
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
        public override async void Ready()
        {
            if (AllowedServerChannelCombo != null)
            {
                ulong serverID = AllowedServerChannelCombo.Item1;
                ulong channelID = AllowedServerChannelCombo.Item2;

                SocketGuild guild = this.Client.GetGuild(serverID);
                SocketTextChannel channel = guild.GetTextChannel(channelID);

                bool didFindAndSet = await ActivePost.FindAndSetPostID(this, channel, 20);
            }
        }
    }
}
