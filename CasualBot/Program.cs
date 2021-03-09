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

                client.OnMessage = (message) =>
                {
                    try
                    {
                        if (message.Content == "bangh")
                        {
                            message.Channel.SendMessageAsync("bongh", true);
                        }
                        else
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
