using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot
{
    public abstract class PawDiscordBotClient : IDisposable
    {
        private string _key;

        public string LogName { get; set; }

        public DiscordSocketClient Client { get; private set; }
        public CommandService CommandService { get; private set; }

        public IPawDiscordBotLogger Logger { get; set; }

        public Action<SocketUserMessage> OnMessage { get; set; }

        public PawDiscordBotClient(string key, string LogName = "DiscordBotClient")
        {
            this._key = key;
        }

        ~PawDiscordBotClient()
        {
            try { if (Client != null) Client.Dispose(); } catch { }
        }

        public async void Start(DiscordSocketConfig socketConfig)
        {
            if (string.IsNullOrEmpty(_key))
                throw new PawDiscordBotException("Client Key cannot be null!");

            Log("Creating Client...");
            Client = new DiscordSocketClient(socketConfig);
            Client.Log += DiscordDotNetLog;

            Log("Logging in...");
            await Client.LoginAsync(TokenType.Bot, _key);

            Log("Starting...");
            await Client.StartAsync();

            Client.MessageReceived += Discord_MessageReceived;
            Client.MessageUpdated += Discord_MessageUpdated;
            Client.Connected += () => { Log("Bot Connected!"); return Task.CompletedTask; };
            Client.Ready += () => { Log("Bot Ready!"); return Task.CompletedTask; };

            await Task.Delay(-1);
        }

        public Task RegisterModule(Type moduleType)
        {
            if (CommandService == null)
            {
                CommandService = new CommandService(new CommandServiceConfig() { LogLevel = LogSeverity.Debug });
                CommandService.Log += DiscordDotNetLog;
            }

            return CommandService.AddModuleAsync(moduleType, null);
        }


        public void AddPawCommand(string trigger, Action act)
        {
        }


        private void Log(string msg)
        {
            if (Logger != null)
                Logger.Log("[CasualClient] " + msg);
        }













        private async Task Discord_MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            /*
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
            */
        }

        private Task Discord_MessageReceived(SocketMessage arg)
        {
            return Task.Factory.StartNew(() =>
            {
                if (arg is SocketUserMessage)
                {
                    SocketUserMessage message = (SocketUserMessage)arg;

                    // Create a number to track where the prefix ends and the command begins
                    int argPos = 0;

                    // Determine if the message is a command based on the prefix and make sure no bots trigger commands
                    /*
                    if (!(message.HasCharPrefix('!', ref argPos)))
                        return;
                    */

                    if (message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                        return;

                    if (message.Author.IsBot)
                        return;

                    if ("!consita".Equals(message.Content))
                    {
                        message.Channel.SendMessageAsync("boop");
                        return;
                    }

                    // Create a WebSocket-based command context based on the message
                    var context = new SocketCommandContext(Client, message);

                    // Execute the command with the command context we just
                    // created, along with the service provider for precondition checks.
                    if (CommandService != null)
                    {
                        IResult res = CommandService.ExecuteAsync(context, argPos, null).Result;

                        if (!res.IsSuccess && OnMessage != null)
                        {
                            OnMessage.Invoke(message);
                        }
                    }
                    else
                    {
                        if (OnMessage != null)
                        {
                            OnMessage.Invoke(message);
                        }
                    }
                }
                else
                {

                }
            });
        }

        private Task DiscordDotNetLog(LogMessage arg)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    if (Logger != null)
                        Logger.Log(arg.Message);
                }
                catch (Exception exc) { if (Debugger.IsAttached) Debugger.Break(); }
            });
        }

        public void Dispose()
        {
            try
            {
                if (Client != null)
                    Client.Dispose();
            }
            catch { }
        }
    }
}
