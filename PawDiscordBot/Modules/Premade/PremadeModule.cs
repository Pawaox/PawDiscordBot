using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawDiscordBot.Modules.Premade
{
    internal abstract class PremadeModule : ModuleBase<SocketCommandContext>, IServiceProvider
    {
        /// <summary>
        /// The type of this command
        /// </summary>
        internal PremadeModuleType ModuleType { get; set; }

        internal string Trigger { get; set; }

        internal PremadeModule() { }
        internal PremadeModule(PremadeModuleType premadeModuleType, string trigger)
        {
            this.ModuleType = premadeModuleType;
            this.Trigger = trigger;
        }

        public object GetService(Type serviceType)
        {
            return this;
        }
    }
}
