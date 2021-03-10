using Discord.WebSocket;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Commands
{
    public class CommandStorage
    {
        private PawDiscordBotClient _client;

        private Dictionary<PremadeCommand, ActivePremadeCommand> _activeFeatures;

        private Dictionary<string, PawDiscordCommandBase> _customCommands;

        public CommandStorage(PawDiscordBotClient client)
        {
            _client = client;
            _activeFeatures = new Dictionary<PremadeCommand, ActivePremadeCommand>();
            _customCommands = new Dictionary<string, PawDiscordCommandBase>();

            AddActiveFeature(new ActivePremadeCommand(PremadeCommand.NONE, "", (m) => { }));
            AddActiveFeature(new ActivePremadeCommand(PremadeCommand.PAUSE, "", PremadePause));
            AddActiveFeature(new ActivePremadeCommand(PremadeCommand.UNPAUSE, "", PremadeUnpause));
            AddActiveFeature(new ActivePremadeCommand(PremadeCommand.TEST_EXCEPTION_NULLPOINTER, "", (m) => { string temp = null; temp = temp.Substring(0, 1); }));
            AddActiveFeature(new ActivePremadeCommand(PremadeCommand.TEST_EXCEPTION_PAWDISCORDBOT, "", (m) => throw new PawDiscordBotUserWarningException("This is a test error")));
        }

        private void AddActiveFeature(ActivePremadeCommand apc)
        {
            if (apc != null)
                _activeFeatures.Add(apc.Command, apc);
        }

        public bool Contains(string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key))
            {
                if (!result && _customCommands != null)
                    result = _customCommands.ContainsKey(key);

                if (!result && _activeFeatures != null)
                {
                    ActivePremadeCommand af = GetActiveFeatureFromKey(key);
                    result = af != null;
                }
            }

            return result;
        }

        public bool Invoke(string key, SocketUserMessage message)
        {
            bool handled = false;

            if (!string.IsNullOrEmpty(key))
            {
                if (_customCommands != null && _customCommands.ContainsKey(key))
                {
                    PawDiscordCommandBase cmd = _customCommands[key];
                    handled = cmd.HandleMessage(message);
                }

                if (!handled)
                {
                    ActivePremadeCommand af = GetActiveFeatureFromKey(key);
                    if (af?.Implementation != null)
                    {
                        af.Implementation.Invoke(message);
                        handled = true;
                    }
                }
            }

            return handled;
        }


        #region Premade Feature Methods
        public void RemovePremadeCommand(PremadeCommand cmd)
        {
            if (_activeFeatures.ContainsKey(cmd))
                _activeFeatures[cmd].Trigger = "";
        }

        public void AddPremadeCommand(PremadeCommand cmd, string trigger)
        {
            if (string.IsNullOrEmpty(trigger))
                return;

            if (_activeFeatures.ContainsKey(cmd))
                _activeFeatures[cmd].Trigger = trigger;
        }

        public PremadeCommand GetPremadeCommandType(string key)
        {
            PremadeCommand found = PremadeCommand.NONE;
            ActivePremadeCommand apc = GetActiveFeatureFromKey(key);

            if (apc != null)
                found = apc.Command;

            return found;
        }
        private ActivePremadeCommand GetActiveFeatureFromKey(string key)
        {
            ActivePremadeCommand result = null;
            if (_activeFeatures != null)
            {
                foreach (ActivePremadeCommand apc in _activeFeatures.Values)
                {
                    if (!string.IsNullOrEmpty(apc?.Trigger) && apc.Trigger.Equals(key))
                    {
                        result = apc;
                        break;
                    }
                }
            }

            return result;
        }

        #region Premade Actions
        private void PremadePause(SocketUserMessage msg)
        {
            if (_activeFeatures.ContainsKey(PremadeCommand.PAUSE))
            {
                ActivePremadeCommand apc = _activeFeatures[PremadeCommand.PAUSE];

                if (apc != null && apc.Trigger.Equals(msg.Content))
                {
                    _client.IgnoreReceivedMessages = true;
                }
            }
        }
        private void PremadeUnpause(SocketUserMessage msg)
        {
            if (_activeFeatures.ContainsKey(PremadeCommand.UNPAUSE))
            {
                ActivePremadeCommand apc = _activeFeatures[PremadeCommand.UNPAUSE];

                if (apc != null && apc.Trigger.Equals(msg.Content))
                {
                    _client.IgnoreReceivedMessages = false;
                }
            }
        }
        #endregion
        #endregion


        #region Add/Get Commands
        public void AddCommand(string cmd, SimpleCommand command)
        {
            if (string.IsNullOrEmpty(cmd))
                return;

            if (!_customCommands.ContainsKey(cmd))
                _customCommands.Add(cmd, command);
            else
                _customCommands[cmd] = command;
        }

        public void AddCommand(ParameterCommand command)
        {
            string key = command?.Settings?.RequiredPrefix;

            if (string.IsNullOrEmpty(key))
                return;

            if (!_customCommands.ContainsKey(key))
                _customCommands.Add(key, command);
            else
                _customCommands[key] = command;

        }
        #endregion
    }
}
