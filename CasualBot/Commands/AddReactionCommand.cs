using Discord;
using Discord.WebSocket;
using PawDiscordBot;
using PawDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualBot.Commands
{
    public class AddReactionToMessageCommand : ParameterCommand
    {
        public AddReactionToMessageCommand() : base(new CommandSettings("addReaction", 2)) { }

        public override bool HandleCommand(PawDiscordBotClient client, SocketUserMessage msg, string[] levels, string[] parameters)
        {
            bool handled = false;

            if (levels.Length != 1)
                return handled;

            if (Settings.ParameterCount != parameters.Length)
                return handled;

            if (msg.Author is SocketGuildUser)
            {
                SocketGuildUser socketUser = (SocketGuildUser)msg.Author;
                foreach (var role in socketUser.Roles)
                {
                    if (role.Name.Equals("Mod"))
                    {
                        var messagetxt = parameters[0];
                        var reactionEmote = parameters[1];
                        ulong messageID = 0;

                        if (ulong.TryParse(messagetxt, out messageID))
                        {
                            var messagetarget = msg.Channel.GetMessageAsync(messageID).Result;
                            if (messagetarget != null)
                            {
                                IEmote parsedEmote = null;
                                if (Emote.TryParse(reactionEmote, out var emote))
                                    parsedEmote = emote;
                                else
                                    parsedEmote = new Emoji(reactionEmote);

                                messagetarget.AddReactionAsync(parsedEmote);

                                msg.DeleteAsync();
                            }
                        }
                    }
                }
            }

            handled = true;
            return handled;
        }
    }
}
