using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PawDiscordBot.Commands;
using PawDiscordBot.Exceptions;
using PawDiscordBot.Modules;
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

        /// <summary>
        /// Will be used as prefix in the Log() calls in this client
        /// </summary>
        public string LogName { get; set; }

        public bool CanReplyWithErrors { get; set; }
        public bool CanReplyWithExceptions { get; set; }
        public bool CanReactToBotMessages { get; set; }
        public bool ReactOnlyIfMentionedFirst { get; set; }

        public HashSet<ulong> UserWhiteList { get; set; }

        /// <summary>
        /// Item1: Guild ID
        /// Item2: Channel ID
        /// </summary>
        public Tuple<ulong, ulong> AllowedServerChannelCombo { get; set; }

        public bool PauseMessaging { get; set; }


        /// <summary>
        /// Discord.NET SocketClient object
        /// </summary>
        public DiscordSocketClient Client { get; private set; }

        /// <summary>
        /// Contains implemented commands
        /// </summary>
        public CommandStorage Commands { get; private set; }

        /// <summary>
        /// Contains implemented modules
        /// </summary>
        public ModuleStorage Modules { get; private set; }

        public IPawDiscordBotLogger Logger { get; set; }

        /// <summary>
        /// Called for messages that are not otherwise handled within this client
        /// </summary>
        public event Action<SocketUserMessage> OnUnhandledMessage;

        public PawDiscordBotClient(string key, string botLogName = "DiscordBotClient")
        {
            this._key = key;
            this.LogName = botLogName;

            CommandService commandServ = new CommandService(new CommandServiceConfig() { LogLevel = LogSeverity.Debug });
            commandServ.Log += DiscordDotNetLog;
            commandServ.CommandExecuted += Discord_CommandExecuted;

            Commands = new CommandStorage(this);
            Modules = new ModuleStorage(this, commandServ);

            UserWhiteList = new HashSet<ulong>();
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

        /// <summary>
        /// Called before Discord.NET Client.StartAsync
        /// </summary>
        public abstract void ConnectionStarting();

        /// <summary>
        /// Called before Discord.NET Client.StartAsync
        /// </summary>
        public abstract void ConnectionStarted();

        /// <summary>
        /// Called on Discord.NET Client.Ready
        /// </summary>
        public abstract void Ready();

        /// <summary>
        /// Starts everything needed to connect 
        /// </summary>
        /// <param name="socketConfig"></param>
        public async void Start(DiscordSocketConfig socketConfig)
        {
            if (string.IsNullOrEmpty(_key))
                throw new PawDiscordBotException("Client Key cannot be null!");

            if (Client != null)
            {
                try { Client.Dispose(); } catch { }
                Client = null;
            }

            Log("Creating Client...");
            Client = new DiscordSocketClient(socketConfig);
            Client.Log += DiscordDotNetLog;

            Log("Logging in...");
            await Client.LoginAsync(TokenType.Bot, _key);

            Log("Starting...");
            ConnectionStarting();
            await Client.StartAsync();
            ConnectionStarted();

            //Connect to events
            Client.MessageReceived += Discord_MessageReceived;
            Client.Connected += () => { Log("Bot Connected!"); return Task.CompletedTask; };
            Client.Ready += () => { Log("Bot Ready!"); Ready(); return Task.CompletedTask; };

            await Task.Delay(-1);
        }



        /// <summary>
        /// Adds LogName as prefix to the message and nullchecks the 'Logger' property
        /// </summary>
        /// <param name="msg"></param>
        private void Log(string msg)
        {
            if (Logger != null)
                Logger.Log(this.LogName + " " + msg);
        }

        /// <summary>
        /// Event: Discord.NET CommandService CommandExecuted
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        private Task Discord_CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            return Task.Factory.StartNew(() =>
            {
            });
        }

        /// <summary>
        /// Event: Discord.NET MessageReceived
        /// </summary>
        /// <param name="arg">The SocketMessage object that triggered the event</param>
        /// <returns></returns>
        private Task Discord_MessageReceived(SocketMessage arg)
        {
            return Task.Factory.StartNew(() =>
            {
                if (arg is SocketUserMessage)
                {
                    SocketUserMessage message = (SocketUserMessage)arg;
                    SocketTextChannel textChannel = null;

                    try
                    {
                        string key = message.Content;

                        if (message.Channel is SocketTextChannel)
                            textChannel = (SocketTextChannel)message.Channel;

                        #region Check if allowed to react
                        if (!CanReactToBotMessages && message.Author.IsBot)
                            return;

                        if (UserWhiteList != null && UserWhiteList.Count > 0 && !UserWhiteList.Contains(message.Author.Id))
                            return;
                        
                        int prefixPosition = 0;
                        message.HasMentionPrefix(Client.CurrentUser, ref prefixPosition);

                        if (ReactOnlyIfMentionedFirst && prefixPosition <= 0)
                            return;

                        if (AllowedServerChannelCombo != null)
                        {
                            ulong allowedServer = AllowedServerChannelCombo.Item1;
                            ulong allowedChannel = AllowedServerChannelCombo.Item2;

                            if (!textChannel.Guild.Id.Equals(allowedServer) || !textChannel.Id.Equals(allowedChannel))
                                return;
                        }
                        #endregion

                        //Remove potential mention prefix and get first actual section of key
                        if (prefixPosition > 0)
                            key = key.Substring(prefixPosition);

                        int spacePos = key.IndexOf(' ');
                        if (spacePos > 0)
                            key = key.Split(' ')[0];

                        if (!string.IsNullOrEmpty(key))
                            key = key.Trim();
                        #region Paused?
                        if (PauseMessaging)
                        {
                            //The only command allowed if paused is unpause
                            if (Commands.GetPremadeCommandType(key) == PremadeCommandType.UNPAUSE)
                                Commands.Invoke(key, message);

                            return;
                        }
                        #endregion

                        bool handled = false;

                        //1. Check if registered in Discord Command Service
                        if (Modules != null)
                        {
                            /*
                            var context = new SocketCommandContext(Client, message);
                            IResult res = CommandService.ExecuteAsync(context, prefixPosition, null).Result;

                            handled = res.IsSuccess;
                            */
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

                        //Final, didn't handle it yet? Pass it on to the OnMessage last resort.
                        if (!handled && OnUnhandledMessage != null)
                        {
                            OnUnhandledMessage.Invoke(message);
                        }
                    }
                    catch(PawDiscordBotException exc)
                    {
                        switch (exc.ExceptionType)
                        {
                            case ExceptionType.BASE:
                                ReplyToException(message, exc.ToString());
                                break;
                            case ExceptionType.WARN_USER:
                                ReplyToError(message, exc.Message);
                                break;
                        }

                    }
                    catch (Exception exc)
                    {
                        ReplyToException(message, exc.ToString());
                    }
                }

                //Should be the last 'else if'
                else if (arg != null)
                {
                    Log("Received message of unhandled type (" + arg.GetType().FullName + ")");
                }
            });
        }

        /// <summary>
        /// Event: When Discord.NET Client 'Log' event is called, call the IPawDiscordBotLogger implementation if present
        /// </summary>
        /// <param name="arg">The LogMessage from Discord.NET</param>
        /// <returns></returns>
        private Task DiscordDotNetLog(LogMessage arg)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    lock (Logger)
                    {
                        if (Logger != null)
                            Logger.Log(arg.Message);
                    }
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
