using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs
{
    internal enum LoggedRaid
    {
        NONE = 0,

        KARAZHAN = 12,
        GRUUL = 11,
        MAG = 10,
        GRUUL_MAG = 9,

        SSC = 8,
        TK = 7,
        SSC_TK = 6,

        HYJAL = 5,
        BT = 4,
        HYJAL_BT = 3,

        ZA = 2,
        SWP = 1
    }
}
