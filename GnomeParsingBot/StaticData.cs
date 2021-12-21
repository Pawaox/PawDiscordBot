using GnomeParsingBot.WarcraftLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot
{
    internal abstract class StaticData
    {
        public const string URL_WARCRAFTLOGS_REPORTS = "https://classic.warcraftlogs.com/reports/";

        public static string WarcraftLogsGuildName { get; set; }
        public static string WarcraftLogsServer { get; set; }
        public static WarcraftLogsClient.Region WarcraftLogsRegion { get; set; }

        public static bool Initialized { get; private set; }

        static StaticData()
        {
            WarcraftLogsGuildName = "";
            WarcraftLogsServer = "";
        }

        public static void Initialize(string guildName, string server, WarcraftLogsClient.Region region)
        {
            WarcraftLogsGuildName = guildName;
            WarcraftLogsServer = server;
            WarcraftLogsRegion = region;

            Initialized = true;
        }
    }
}
