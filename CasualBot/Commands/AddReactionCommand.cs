using Discord;
using Discord.WebSocket;
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
        public AddReactionToMessageCommand() : base(new CustomCommandSettings("addReaction", 2)) { }

        public override bool HandleCommand(SocketUserMessage message, string[] levels, string[] parameters)
        {
            bool handled = false;

            if (levels.Length != 1)
                return handled;

            if (Settings.ParameterCount != parameters.Length)
                return handled;

            if (message.Author is SocketGuildUser)
            {
                SocketGuildUser socketUser = (SocketGuildUser)message.Author;
                foreach (var role in socketUser.Roles)
                {
                    if (role.Name.Equals("Mod"))
                    {
                        var messagetxt = parameters[0];
                        var reactionEmote = parameters[1];
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

            handled = true;
            return handled;
        }
    }
}
