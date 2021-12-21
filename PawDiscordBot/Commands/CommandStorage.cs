using Discord.WebSocket;
using PawDiscordBot.Commands.Premade;
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

        private Dictionary<PremadeCommandType, PremadeCommand> _availableFeatures;

        private Dictionary<string, PawDiscordCommandBase> _customCommands;

        public CommandStorage(PawDiscordBotClient client)
        {
            _client = client;
            _availableFeatures = new Dictionary<PremadeCommandType, PremadeCommand>();
            _customCommands = new Dictionary<string, PawDiscordCommandBase>();

            CreateAvailableFeature(new BasicPremadeCommand(PremadeCommandType.NONE, "", null));
            CreateAvailableFeature(new PausePremadeCommand(PremadeCommandType.PAUSE, "", true));
            CreateAvailableFeature(new PausePremadeCommand(PremadeCommandType.UNPAUSE, "", false));
            CreateAvailableFeature(new PurgePremadeCommand(""));
            CreateAvailableFeature(new BasicPremadeCommand(PremadeCommandType.TEST_EXCEPTION_NULLPOINTER, "", (c, m) => { string temp = null; temp = temp.Substring(0, 1); }));
            CreateAvailableFeature(new BasicPremadeCommand(PremadeCommandType.TEST_EXCEPTION_PAWDISCORDBOT, "", (c, m) => throw new PawDiscordBotException(ExceptionType.WARN_USER, "This is a test error")));
        }

        private void CreateAvailableFeature(PremadeCommand apc)
        {
            if (apc != null)
                _availableFeatures.Add(apc.CommandType, apc);
        }

        public bool Contains(string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key))
            {
                if (!result && _customCommands != null)
                    result = _customCommands.ContainsKey(key);

                if (!result && _availableFeatures != null)
                {
                    PremadeCommand pm = GetActiveFeatureFromKey(key);
                    result = pm != null;
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
                    handled = cmd.HandleMessage(_client, message);
                }

                if (!handled)
                {
                    PremadeCommand pm = GetActiveFeatureFromKey(key);

                    if (pm != null)
                        handled = pm.HandleMessage(_client, message);
                }
            }

            return handled;
        }


        #region Premade Feature Methods
        public void RemovePremadeCommand(PremadeCommandType cmd)
        {
            if (_availableFeatures.ContainsKey(cmd))
                _availableFeatures[cmd].Trigger = "";
        }

        public void AddPremadeCommand(PremadeCommandType cmd, string trigger)
        {
            if (string.IsNullOrEmpty(trigger))
                return;

            if (_availableFeatures.ContainsKey(cmd))
                _availableFeatures[cmd].Trigger = trigger;
        }

        public PremadeCommandType GetPremadeCommandType(string key)
        {
            PremadeCommandType found = PremadeCommandType.NONE;
            PremadeCommand apc = GetActiveFeatureFromKey(key);

            if (apc != null)
                found = apc.CommandType;

            return found;
        }
        private PremadeCommand GetActiveFeatureFromKey(string key)
        {
            PremadeCommand result = null;
            if (_availableFeatures != null)
            {
                foreach (PremadeCommand apc in _availableFeatures.Values)
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
