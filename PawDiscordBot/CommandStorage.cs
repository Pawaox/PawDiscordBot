using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot
{
    public class CommandStorage
    {
        private PawDiscordBotClient _client;

        private Dictionary<PremadeFeature, ActiveFeature> _activeFeatures;
        private Dictionary<string, Action<SocketUserMessage>> _customCommands;

        public CommandStorage(PawDiscordBotClient client)
        {
            _client = client;
            _activeFeatures = new Dictionary<PremadeFeature, ActiveFeature>();
            _customCommands = new Dictionary<string, Action<SocketUserMessage>>();

            _activeFeatures.Add(PremadeFeature.NONE, new ActiveFeature(PremadeFeature.NONE, "", (m) => { }));
            _activeFeatures.Add(PremadeFeature.PAUSE, new ActiveFeature(PremadeFeature.PAUSE, "", PremadePause));
            _activeFeatures.Add(PremadeFeature.UNPAUSE, new ActiveFeature(PremadeFeature.UNPAUSE, "", PremadeUnpause));
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
                    ActiveFeature af = GetActiveFeatureFromKey(key);
                    result = af != null;
                }
            }

            return result;
        }

        public bool Invoke(string key, SocketUserMessage message)
        {
            bool invoked = false;

            if (!string.IsNullOrEmpty(key))
            {
                if (_customCommands != null && _customCommands.ContainsKey(key))
                {
                    _customCommands[key].Invoke(message);
                    invoked = true;
                }

                if (!invoked)
                {
                    ActiveFeature af = GetActiveFeatureFromKey(key);
                    if (af?.Implementation != null)
                    {
                        af.Implementation.Invoke(message);
                        invoked = true;
                    }
                }
            }

            return invoked;
        }


        #region Premade Feature Methods
        public void RemovePremadeCommand(PremadeFeature cmd)
        {
            if (_activeFeatures.ContainsKey(cmd))
                _activeFeatures[cmd].Trigger = "";
        }

        public void AddPremadeCommand(PremadeFeature cmd, string trigger)
        {
            if (string.IsNullOrEmpty(trigger))
                return;

            if (_activeFeatures.ContainsKey(cmd))
                _activeFeatures[cmd].Trigger = trigger;
        }

        public PremadeFeature GetFeatureType(string key)
        {
            PremadeFeature found = PremadeFeature.NONE;
            ActiveFeature af = GetActiveFeatureFromKey(key);

            if (af != null)
                found = af.Feature;

            return found;
        }
        private ActiveFeature GetActiveFeatureFromKey(string key)
        {
            ActiveFeature result = null;
            if (_activeFeatures != null)
            {
                foreach (ActiveFeature af in _activeFeatures.Values)
                {
                    if (!string.IsNullOrEmpty(af?.Trigger) && af.Trigger.Equals(key))
                    {
                        result = af;
                        break;
                    }
                }
            }

            return result;
        }

        #region Premade Actions
        private void PremadePause(SocketUserMessage msg)
        {
            if (_activeFeatures.ContainsKey(PremadeFeature.PAUSE))
            {
                ActiveFeature af = _activeFeatures[PremadeFeature.PAUSE];

                if (af != null && af.Trigger.Equals(msg.Content))
                {
                    _client.IgnoreReceivedMessages = true;
                }
            }
        }
        private void PremadeUnpause(SocketUserMessage msg)
        {
            if (_activeFeatures.ContainsKey(PremadeFeature.UNPAUSE))
            {
                ActiveFeature af = _activeFeatures[PremadeFeature.UNPAUSE];

                if (af != null && af.Trigger.Equals(msg.Content))
                {
                    _client.IgnoreReceivedMessages = false;
                }
            }
        }
        #endregion
        #endregion


        #region Add/Get Commands
        private void AddCommand(string cmd, Action<SocketUserMessage> command)
        {
            if (string.IsNullOrEmpty(cmd))
                return;

            if (!_customCommands.ContainsKey(cmd))
                _customCommands.Add(cmd, command);
            else
                _customCommands[cmd] = command;
        }

        private Action<SocketUserMessage> GetCommand(string cmd)
        {
            Action<SocketUserMessage> result = null;

            if (_customCommands.ContainsKey(cmd))
                result = _customCommands[cmd];

            return result;
        }


        private Action<SocketUserMessage> this[string cmd]
        {
            get { return GetCommand(cmd); }
            set { AddCommand(cmd, value); }
        }
        #endregion

        public class ActiveFeature
        {
            public PremadeFeature Feature { get; set; }
            public string Trigger { get; set; }
            public Action<SocketUserMessage> Implementation { get; set; }

            public ActiveFeature() { }
            public ActiveFeature(PremadeFeature feat, string trigger, Action<SocketUserMessage> implementation)
            {
                this.Feature = feat;
                this.Trigger = Trigger;
                this.Implementation = implementation;
            }
        }


        public enum PremadeFeature
        {
            NONE,
            PAUSE,
            UNPAUSE
        }
    }
}
