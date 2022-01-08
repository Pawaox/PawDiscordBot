using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs.RequestResponse
{
    internal class GetConsumableUseRequest
    {
        public int SpellID { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
    }
}
