using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualBot.Modules
{
	public class NoPrefixModule : ModuleBase<SocketCommandContext>
	{
		[Command("say")]
		[Summary("Echoes a message.")]
		public Task EchoAsync([Remainder][Summary("The text to echo")] string echo) => ReplyAsync(echo);

		[Command("ping")]
		[Summary("pingpong")]
		public Task PingPongAsync() => ReplyAsync("pong");
	}
}
