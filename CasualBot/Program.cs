using CasualBot.Modules;
using Discord;
using Discord.WebSocket;
using PawDiscordBot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CasualBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string botKeyPath = "C:/discordclient.txt";
                string botLogPath = "C:/discordBotLog.txt";


                string botKey = "";
                if (File.Exists(botKeyPath))
                    botKey = File.ReadAllText(botKeyPath);


                DiscordSocketConfig dsCfg = new DiscordSocketConfig();
                dsCfg.WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance;

                CasualBotClient client = new CasualBotClient(botKey);
                client.Logger = new CasualBotLogger(botLogPath, true);
                

                // client.RegisterModule(typeof(NoPrefixModule));
                client.Start(dsCfg);

                client.Client.UserUpdated += Client_UserUpdated;

                client.OnMessage = (message) =>
                {
                    try
                    {
                        // bangh - bongh
                        if (message.Content == "bangh")
                        {
                            message.Channel.SendMessageAsync("bongh", true);
                        }
                        // addReaction() - addReaction MessageID Emote
                        else if (message.Content.StartsWith("addReaction"))
                        {
                            string[] strSplit = message.Content.Split(' ');
                            if (strSplit.Length <= 0 || strSplit.Length != 3 || strSplit[0] != "addReaction")
                            {
                                return;
                            }
                            if (message.Author is SocketGuildUser)
                            {
                                SocketGuildUser socketUser = (SocketGuildUser)message.Author;
                                foreach (var role in socketUser.Roles)
                                {
                                    if (role.Name.Equals("Mod"))
                                    {
                                        var messagetxt = strSplit[1];
                                        var reactionEmote = strSplit[2];
                                        ulong messageID = 0;


                                        if (ulong.TryParse(messagetxt, out messageID))
                                        {
                                            var messagetarget = message.Channel.GetMessageAsync(messageID).Result;
                                            if (messagetarget != null)
                                            {
                                                IEmote parsedEmote = null;
                                                if (Emote.TryParse(reactionEmote, out var emote))
                                                    parsedEmote = emote;
                                                else
                                                    parsedEmote = new Emoji(reactionEmote);

                                                messagetarget.AddReactionAsync(parsedEmote);

                                                message.DeleteAsync();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Debugger.IsAttached)
                            Debugger.Break();
                    }

                };





                ConsoleColor oldColor = Console.ForegroundColor;
                bool doRun = true;
                while (doRun)
                {
                    string input = Console.ReadLine();

                    switch (input?.ToLower() ?? "")
                    {
                        case "stop":
                            doRun = false;

                            oldColor = ChangeForeground(ConsoleColor.Yellow);
                            client.Logger.Log("Input '" + input + "' - STOPPING...");
                            break;
                        case "exit":
                            doRun = false;

                            oldColor = ChangeForeground(ConsoleColor.Yellow);
                            client.Logger.Log("Input '" + input + "' - STOPPING...");
                            break;
                    }
                }

                ChangeForeground(oldColor);
                Thread.Sleep(4000);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                Console.Write("Finished, Press <ENTER> to lose");
                Console.ReadLine();
            }
        }

        private static Task Client_UserUpdated(SocketUser arg1, SocketUser arg2)
        {
            //do consita stuff
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

            return Task.CompletedTask;
        }

        static ConsoleColor ChangeForeground(ConsoleColor newColor)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = newColor;
            return oldColor;
        }
        static ConsoleColor ChangeBackground(ConsoleColor newColor)
        {
            ConsoleColor oldColor = Console.BackgroundColor;
            Console.BackgroundColor = newColor;
            return oldColor;
        }
    }
}
