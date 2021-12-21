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

            AllowedServerChannelCombo = new Tuple<ulong, ulong>(701098779126005820, 816316160633995274);

            string prefix = "!_";
            //Modules.RegisterPremadeModule(PawDiscordBot.Modules.PremadeModuleType.MUSIC);

            Commands.AddPremadeCommand(PremadeCommandType.PAUSE, prefix + "pause");
            Commands.AddPremadeCommand(PremadeCommandType.UNPAUSE, prefix + "unpause");
            Commands.AddPremadeCommand(PremadeCommandType.PURGE, prefix + "purge");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_PAWDISCORDBOT, prefix + "crash_a");
            Commands.AddPremadeCommand(PremadeCommandType.TEST_EXCEPTION_NULLPOINTER, prefix + "crash_b");

            Commands.AddCommand(new CreatePostCommand(prefix));
            Commands.AddCommand(new AddRaidLogCommand(prefix));

            //Commands.AddCommand("createPost", new SimpleCommand(msg => msg.Channel.SendMessageAsync("Text", 
        }


        public override void ConnectionStarting()
        {
        }

        public async override void ConnectionStarted()
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

                bool done = false;
                await foreach (var messages in channel.GetMessagesAsync(10))
                {
                    if (done)
                        break;

                    foreach (IMessage msg in messages)
                    {
                        if (done)
                            break;

                        if (msg.Author.Id.Equals(Client.CurrentUser.Id))
                        {
                            string[] msgLines = msg.Content.Split(Environment.NewLine);

                            if (msgLines.Length > 0)
                            {
                                string firstLine = msgLines[0];
                                if (firstLine.IndexOf("Logs", StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    ActivePost.ResetMessage(msg.Id, msgLines[0]);

                                    for (int i = 1; i < msgLines.Length; i++)
                                    {
                                        string line = msgLines[i]?.Trim() ?? "";

                                        LoggedRaid raid = LoggedRaid.NONE;
                                        string logID = "";

                                        if (!string.IsNullOrEmpty(line))
                                        {
                                            raid = ActivePost.GetRaidFromLine(line);
                                            if (raid != LoggedRaid.NONE)
                                            {
                                                int sep = line.IndexOf(':');
                                                string work = line.Substring(sep + 1).Trim();
                                                work = work.Substring(1, work.Length - 3);
                                                sep = work.LastIndexOf('/');
                                                logID = work.Substring(sep + 1);
                                            }
                                        }

                                        if (raid != LoggedRaid.NONE)
                                        {
                                            ActivePost.AddLog(raid, logID);
                                        }
                                    }

                                    done = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!done)
                {
                    var temp = ActivePost.RaidLogs;

                }

                // this.Client.GetGuild(server).GetChannel(channel).
            }
        }
    }
}
