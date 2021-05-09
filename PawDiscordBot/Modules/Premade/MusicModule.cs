using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Modules.Premade
{
	internal class MusicModule : PremadeModule
	{
		private PawDiscordBotClient _client { get; set; }

        public MusicModule(PawDiscordBotClient client, PremadeModuleType premadeModuleType, string trigger) : base(premadeModuleType, trigger)
        {
			this._client = client;
        }

        [Command("Play")]
		public Task Play(object channel, string input) => ReplyAsync("Play");

		[Command("Stop")]
		public Task Stop() => ReplyAsync("Stop");

		[Command("Pause")]
		public Task Pause() => ReplyAsync("Pause");

		[Command("Resume")]
		public Task Resume() => ReplyAsync("Resume");

		[Command("Skip")]
		public Task Skip() => ReplyAsync("Skip");

		[Command("Volume")]
		public Task Volume(int percent) => ReplyAsync("Volume");
    }
}
