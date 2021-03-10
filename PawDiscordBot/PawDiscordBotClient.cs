using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PawDiscordBot.Commands;
using PawDiscordBot.Exceptions;
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
        public bool CanReplyWithErrors { get; set; }
        public bool CanReplyWithExceptions { get; set; }

        private CommandService CommandService { get; set; }

        public DiscordSocketClient Client { get; private set; }


        public CommandStorage Commands { get; private set; }
        public IPawDiscordBotLogger Logger { get; set; }

        public event Action<SocketUserMessage> OnMessage;

        public bool IgnoreReceivedMessages { get; set; }

        public PawDiscordBotClient(string key, string LogName = "DiscordBotClient")
        {
            this._key = key;
            Commands = new CommandStorage(this);
        }

        public void ReplyToError(SocketUserMessage message, string reply)
        {
            if (this.CanReplyWithErrors)
                message.Channel.SendMessageAsync("ERROR: " + reply);
        }
        public void ReplyToException(SocketUserMessage message, string reply)
        {
            if (this.CanReplyWithExceptions)
                message.Channel.SendMessageAsync(reply);
        }

        ~PawDiscordBotClient()
        {
            try { if (Client != null) Client.Dispose(); } catch { }
        }

        public abstract void ConnectionStarting();
        public abstract void ConnectionStarted();

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
            ConnectionStarting();
            await Client.StartAsync();
            ConnectionStarted();

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
                    try
                    {
                        string key = message.Content;

                        int spacePos = key.IndexOf(' ');
                        if (spacePos > 0)
                            key = key.Split(' ')[0];

                        if (IgnoreReceivedMessages)
                        {
                            if (Commands.GetPremadeCommandType(key) == PremadeCommand.UNPAUSE)
                                Commands.Invoke(key, message);

                            return;
                        }

                        // Create a number to track where the prefix ends and the command begins
                        int argPos = 0;

                        if (message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                            return;

                        if (message.Author.IsBot)
                            return;

                        bool handled = false;

                        //1. Check if registered in Discord Command Service
                        if (CommandService != null)
                        {
                            var context = new SocketCommandContext(Client, message);
                            IResult res = CommandService.ExecuteAsync(context, argPos, null).Result;

                            handled = res.IsSuccess;
                        }

                        //2. Check if registered in custom CommandStorage
                        if (!handled && Commands != null)
                        {
                            if (Commands.Contains(key))
                            {
                                Commands.Invoke(key, message);
                                handled = true;
                            }
                        }


                        //Final, didn't handle it yet? Pass it on.
                        if (!handled && OnMessage != null)
                        {
                            OnMessage.Invoke(message);
                        }
                    }
                    catch (PawDiscordBotUserWarningException warnExc)
                    {
                        try
                        {
                            ReplyToError(message, warnExc.Message);
                        }
                        catch { }
                    }
                    catch(PawDiscordBotException exc)
                    {
                        ReplyToException(message, exc.ToString());
                    }
                    catch (Exception exc)
                    {
                        ReplyToException(message, exc.ToString());
                    }
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
