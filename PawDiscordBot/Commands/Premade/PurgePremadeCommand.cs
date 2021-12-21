using Discord;
using Discord.Rest;
using Discord.WebSocket;
using PawDiscordBot.Commands.Premade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    public class PurgePremadeCommand : PremadeCommand
    {
        public PurgePremadeCommand(string trigger) : base(PremadeCommandType.PURGE, trigger)
        {
        }

        public override bool HandleMessage(PawDiscordBotClient client, SocketUserMessage message)
        {
            bool handled = false;

            string content = message.Content;
            string[] splts = content.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (splts.Length == 2)
            {
                if (int.TryParse(splts[1], out int countToPurge))
                {
                    RestUserMessage reply = null;
                    if (countToPurge > 100)
                    {
                        countToPurge = 100;
                        reply = message.Channel.SendMessageAsync("Cannot purge more than 100, purging 100 instead").Result;
                    }
                    else if (countToPurge < 100)
                        countToPurge++;

                    var messagesToDelete = message.Channel.GetMessagesAsync(countToPurge).FlattenAsync().Result;

                    foreach (IMessage msg in messagesToDelete)
                    {
                        System.Threading.Thread.Sleep(1000);
                        msg.DeleteAsync();
                    }
                    
                    if (reply != null)
                        reply.DeleteAsync();
                }
            }

            return handled;
        }
    }
}
