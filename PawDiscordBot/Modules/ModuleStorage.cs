using Discord.Commands;
using Discord.WebSocket;
using PawDiscordBot.Commands;
using PawDiscordBot.Commands.Premade;
using PawDiscordBot.Exceptions;
using PawDiscordBot.Modules.Premade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Modules
{
    public class ModuleStorage
    {
        /// <summary>
        /// Discord.NET CommandService object
        /// </summary>
        private CommandService _service;
        private PawDiscordBotClient _client;


        private Dictionary<PremadeModuleType, PremadeModule> _availableModules;

        public ModuleStorage(PawDiscordBotClient client, CommandService service)
        {
            _client = client;
            _service = service;
            _availableModules = new Dictionary<PremadeModuleType, PremadeModule>();

            //CreateAvailableModule(new PremadeModule(PremadeModuleType.NONE, null));
            CreateAvailableModule(new MusicModule(_client, PremadeModuleType.MUSIC, ""));
        }

        private void CreateAvailableModule(PremadeModule pm)
        {
            if (pm != null)
            {
                _availableModules.Add(pm.ModuleType, pm);
            }
        }


        public Task<ModuleInfo> RegisterPremadeModule(PremadeModuleType type)
        {
            if (_service == null)
                return Task.Factory.StartNew<ModuleInfo>(() => { return null; });

            return Task.Factory.StartNew(() =>
            {
                ModuleInfo mif = null;

                if (_availableModules.ContainsKey(type))
                {
                    PremadeModule pm = _availableModules[type];

                    mif = _service.AddModuleAsync(typeof(MusicModule), null).Result;
                    if (mif != null)
                        _availableModules.Add(pm.ModuleType, pm);
                }

                return mif;
            });
        }

        /*
        public bool Contains(string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key))
            {
                if (!result && _customCommands != null)
                    result = _customCommands.ContainsKey(key);

                if (!result && _availableModules != null)
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
        */

        #region Premade Feature Methods
        /*
        public void RemovePremadeCommand(PremadeCommandType cmd)
        {
            if (_availableModules.ContainsKey(cmd))
                _availableModules[cmd].Trigger = "";
        }

        public void AddPremadeCommand(PremadeCommandType cmd, string trigger)
        {
            if (string.IsNullOrEmpty(trigger))
                return;

            if (_availableModules.ContainsKey(cmd))
                _availableModules[cmd].Trigger = trigger;
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
            if (_availableModules != null)
            {
                foreach (PremadeCommand apc in _availableModules.Values)
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
        */
        #endregion
        /*

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
        */
    }
}
