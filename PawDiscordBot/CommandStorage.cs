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

        private Dictionary<PremadeFeature, string> _dicMapping;
        private Dictionary<string, Action<SocketUserMessage>> _dicCommands;

        public CommandStorage(PawDiscordBotClient client)
        {
            _client = client;
            _dicMapping = new Dictionary<PremadeFeature, string>();
            _dicCommands = new Dictionary<string, Action<SocketUserMessage>>();
        }

        public void RemovePremadeCommand(PremadeFeature cmd)
        {
            if (_dicMapping.ContainsKey(cmd))
            {
                string key = _dicMapping[cmd];
                _dicMapping.Remove(cmd);

                if (_dicCommands.ContainsKey(key))
                    _dicCommands.Remove(key);
            }
        }

        public void AddPremadeCommand(PremadeFeature cmd, string trigger)
        {
            if (string.IsNullOrEmpty(trigger))
                return;

            if (!_dicCommands.ContainsKey(trigger))
                _dicCommands.Add(trigger, PremadeSleepAndWake);
            else
                _dicCommands[trigger] = PremadeSleepAndWake;

            if (!_dicMapping.ContainsKey(cmd))
                _dicMapping.Add(cmd, trigger);
            else
                _dicMapping[cmd] = trigger;
        }
        public bool Contains(string key)
        {
            if (_dicCommands == null || string.IsNullOrEmpty(key))
                return false;
            return _dicCommands.ContainsKey(key);
        }

        public void Invoke(string key, SocketUserMessage message)
        {
            if (_dicCommands == null || string.IsNullOrEmpty(key))
                return;

            _dicCommands[key].Invoke(message);
        }

        public bool IsFeature(string key, PremadeFeature feature)
        {
            bool result = false;

            if (_dicMapping.ContainsKey(feature))
                result = _dicMapping[feature].Equals(key);

            return result;
        }






        private void PremadeSleepAndWake(SocketUserMessage msg)
        {
            string key = "";
            if (_dicMapping.ContainsKey(PremadeFeature.PAUSE))
            {
                key = _dicMapping[PremadeFeature.PAUSE];

                if (!string.IsNullOrEmpty(key) && key.Equals(msg.Content))
                {
                    _client.IgnoreReceivedMessages = true;
                }
            }
            if (_dicMapping.ContainsKey(PremadeFeature.UNPAUSE))
            {
                key = _dicMapping[PremadeFeature.UNPAUSE];

                if (!string.IsNullOrEmpty(key) && key.Equals(msg.Content))
                {
                    _client.IgnoreReceivedMessages = false;
                }
            }
        }


        private void AddCommand(string cmd, Action<SocketUserMessage> command)
        {
            if (string.IsNullOrEmpty(cmd))
                return;

            if (!_dicCommands.ContainsKey(cmd))
                _dicCommands.Add(cmd, command);
            else
                _dicCommands[cmd] = command;
        }

        private Action<SocketUserMessage> GetCommand(string cmd)
        {
            Action<SocketUserMessage> result = null;

            if (_dicCommands.ContainsKey(cmd))
                result = _dicCommands[cmd];

            return result;
        }


        private Action<SocketUserMessage> this[string cmd]
        {
            get { return GetCommand(cmd); }
            set { AddCommand(cmd, value); }
        }

        public enum PremadeFeature
        {
            PAUSE,
            UNPAUSE
        }
    }
}
