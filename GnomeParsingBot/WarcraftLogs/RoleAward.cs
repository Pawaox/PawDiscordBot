using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs
{
    public static class RoleAward
    {
        public static Dictionary<AwardType, ulong> RoleIDs { get; set; }

        public enum AwardType
        {
            DEATHS,
            BOSSDAMAGE,
            TRASHDAMAGE,
            AVOIDABLEDAMAGETAKEN_LEAST,
            AVOIDABLEDAMAGETAKEN_MOST,
            CONSUMES,
            ENGINEERING,
            DRUMSCORE,
            CHICKEN,


            HEALING,
            HEALER_PACIFISM
        }
    }
}
