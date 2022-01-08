using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs.RequestResponse
{
    internal class GetDamageDoneResponse
    {
        public Dictionary<string, long> TotalDamage { get; set; }
        public Dictionary<string, long> TrashDamage { get; set; }
        public Dictionary<string, long> BossDamage { get; set; }
    }
}
