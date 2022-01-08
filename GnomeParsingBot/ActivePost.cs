using Discord;
using Discord.WebSocket;
using GnomeParsingBot.WarcraftLogs;
using PawDiscordBot;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot
{
    internal abstract class ActivePost
    {
        private ActivePost() { }

        public static ulong? PostID { get; set; }

        public static string DateLine { get; set; }
        public static Dictionary<LoggedRaid, Dictionary<int, LoggedRaidData>> RaidLogs { get; set; }

        static ActivePost()
        {
            DateLine = "";
            RaidLogs = new Dictionary<LoggedRaid, Dictionary<int, LoggedRaidData>>();
        }

        public static void AddLog(LoggedRaid raid, LoggedRaidData log)
        {
            if (!RaidLogs.ContainsKey(raid))
                RaidLogs.Add(raid, new Dictionary<int, LoggedRaidData>());

            int raidCount = RaidLogs[raid].Count;

            RaidLogs[raid].Add(raidCount + 1, log);
        }

        public static void EditLog(LoggedRaid raid, int index, LoggedRaidData log)
        {
            if (RaidLogs.ContainsKey(raid))
            {
                if (RaidLogs[raid].ContainsKey(index))
                {
                    RaidLogs[raid][index] = log;
                }
            }
        }

        public static async void RewriteMessage(string textMessage, PawDiscordBotClient client, ISocketMessageChannel channel)
        {
            if (!PostID.HasValue)
                return;

            Embed embed = GenerateEmbed();

            await channel.ModifyMessageAsync(PostID.Value, (prop) =>
            {
                prop.Content = textMessage;
                prop.Embed = embed;
            });
        }

        public static Embed GenerateEmbed()
        {
            EmbedBuilder b = new EmbedBuilder();
            b.WithTitle(DateLine);

            List<LoggedRaid> raids = new List<LoggedRaid>();
            raids.AddRange(RaidLogs.Keys);
            raids.Sort((a, b) => ((int)a).CompareTo((int)b));

            bool inline = false;
            LoggedRaid lastRaid = LoggedRaid.NONE;

            foreach (LoggedRaid lRaid in raids)
            {
                Dictionary<int, LoggedRaidData> raidDic = RaidLogs[lRaid];

                foreach (var raid in raidDic)
                {
                    inline = false;

                    LoggedRaidData data = raid.Value;

                    string field = CreateLineFromRaid(lRaid, raid.Key, true, RaidLogs[lRaid].Count == 1);
                    string wcl = StaticData.URL_WARCRAFTLOGS_BROWSERREPORTS + data.LogID + "/";
                    string analytics = data.CLA_URL ?? "";
                    string performance = data.RPB_URL ?? "";

                    string finalText = "[Logs](" + wcl + ")";

                    if (!string.IsNullOrEmpty(analytics))
                        finalText += " - " + "[Analytics](" + analytics + ")";

                    if (!string.IsNullOrEmpty(performance))
                        finalText += " - " + "[Performance](" + performance + ")";

                    if (lRaid == LoggedRaid.SSC || lRaid == LoggedRaid.TK)
                        inline = true;

                    if (lRaid == LoggedRaid.GRUUL || lRaid == LoggedRaid.MAG)
                        inline = true;

                    if (lRaid == LoggedRaid.HYJAL || lRaid == LoggedRaid.BT)
                        inline = true;

                    if (lRaid.ToString().Contains("_"))
                        inline = false;

                    b.AddField(field, finalText, inline);
                }

                lastRaid = lRaid;
            }

            return b.Build();
        }

        public static string GenerateMessage()
        {
            StringBuilder newText = new StringBuilder();
            newText.AppendLine(DateLine);

            bool didSection = false;
            if (RaidLogs.ContainsKey(LoggedRaid.SWP))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.SWP));
            }
            if (RaidLogs.ContainsKey(LoggedRaid.ZA))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.ZA));
            }

            if (didSection)
                newText.AppendLine();
            didSection = false;

            if (RaidLogs.ContainsKey(LoggedRaid.BT))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.BT));
            }
            if (RaidLogs.ContainsKey(LoggedRaid.HYJAL))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.HYJAL));
            }
            if (RaidLogs.ContainsKey(LoggedRaid.HYJAL_BT))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.HYJAL_BT));
            }

            if (didSection)
                newText.AppendLine();
            didSection = false;

            if (RaidLogs.ContainsKey(LoggedRaid.TK))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.TK));
            }
            if (RaidLogs.ContainsKey(LoggedRaid.SSC))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.SSC));
            }
            if (RaidLogs.ContainsKey(LoggedRaid.SSC_TK))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.SSC_TK));
            }

            if (didSection)
                newText.AppendLine();
            didSection = false;

            if (RaidLogs.ContainsKey(LoggedRaid.MAG))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.MAG));
            }
            if (RaidLogs.ContainsKey(LoggedRaid.GRUUL))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.GRUUL));
            }
            if (RaidLogs.ContainsKey(LoggedRaid.GRUUL_MAG))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.GRUUL_MAG));
            }

            if (didSection)
                newText.AppendLine();
            didSection = false;

            if (RaidLogs.ContainsKey(LoggedRaid.KARAZHAN))
            {
                didSection = true;
                newText.Append(GenerateMessageFor(LoggedRaid.KARAZHAN));
            }

            return newText.ToString();
        }

        private static string GenerateMessageFor(LoggedRaid r)
        {
            StringBuilder sb = new StringBuilder();

            if (RaidLogs.ContainsKey(r))
            {
                for (int index = 1; index <= RaidLogs[r].Count; index++)
                {
                    sb.Append(CreateLineFromRaid(r, index, false, RaidLogs[r].Count == 1)).AppendLine("<" + StaticData.URL_WARCRAFTLOGS_BROWSERREPORTS + RaidLogs[r][index] + "/>");
                }
            }

            return sb.ToString();
        }

        public static void ResetMessage(ulong id, string firstLine)
        {
            ClearData();
            PostID = id;
            DateLine = firstLine;
        }

        public static void FetchFreshDataFromWarcraftlogs(ulong id, string firstLine, DateTime dateToFetch)
        {
            if (!StaticData.Initialized)
                throw new PawDiscordBotException(ExceptionType.WARN_USER, "Bot not properly configured, StaticData is not initialized!");

            ResetMessage(id, firstLine);

            using (WarcraftLogsClient logClient = new WarcraftLogsClient(StaticData.PATH_KEY_BASE))
            {
                var dict = logClient.GetLogs(StaticData.WarcraftLogsGuildName, StaticData.WarcraftLogsServer, StaticData.WarcraftLogsRegion, dateToFetch);

                foreach (var keyPair in dict)
                {
                    if (!RaidLogs.ContainsKey(keyPair.Key))
                        RaidLogs.Add(keyPair.Key, new Dictionary<int, LoggedRaidData>());

                    int count = 1;
                    foreach (string log in keyPair.Value)
                    {
                        RaidLogs[keyPair.Key].Add(count++, new LoggedRaidData(keyPair.Key, log));
                    }
                }
            }
        }

        private static void ClearData()
        {
            RaidLogs = new Dictionary<LoggedRaid, Dictionary<int, LoggedRaidData>>();
        }


        public async static Task<bool> FindAndSetPostID(PawDiscordBotClient client, ISocketMessageChannel channel, int msgCountToSearch)
        {
            bool done = false;
            await foreach (var messages in channel.GetMessagesAsync(msgCountToSearch))
            {
                if (done)
                    break;

                foreach (IMessage msg in messages)
                {
                    if (done)
                        break;

                    if (msg.Author.Id.Equals(client.Client.CurrentUser.Id))
                    {
                        string[] msgLines = msg.Content.Split(Environment.NewLine);

                        if (msgLines.Length > 0)
                        {
                            if (msg.Embeds.Count > 0)
                            {
                                IEmbed emb = msg.Embeds.FirstOrDefault();
                                if (emb?.Fields != null && emb.Title.IndexOf("Logs", StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    ActivePost.ResetMessage(msg.Id, emb.Title);

                                    for (int i = 0; i < emb.Fields.Length; i++)
                                    {
                                        EmbedField ef = emb.Fields[i];

                                        WarcraftLogs.LoggedRaid raid = ActivePost.GetRaidFromLine(ef.Name);
                                        ActivePost.LoggedRaidData lrd = null;

                                        if (raid != LoggedRaid.NONE)
                                        {
                                            string[] links = ef.Value.Split('[', StringSplitOptions.RemoveEmptyEntries);

                                            lrd = new LoggedRaidData(raid);

                                            foreach (string l in links)
                                            {
                                                int index = l.IndexOf(']');
                                                string workHead = l.Substring(0, index);
                                                string workVal = workHead;

                                                index = l.IndexOf('(');
                                                workVal = l.Substring(index + 1);
                                                index = workVal.IndexOf(')');
                                                workVal = workVal.Substring(0, index - 1);

                                                if (workHead.Contains("Logs")) //WarcraftLogs
                                                {
                                                    index = workVal.LastIndexOf('/');
                                                    lrd.LogID = workVal.Substring(index + 1);
                                                }
                                                else if (workHead.Contains("Analytics")) //cla
                                                {
                                                    lrd.CLA_URL = workVal;
                                                    index = lrd.CLA_URL.LastIndexOf('/');
                                                    lrd.CLA_SheetID = lrd.CLA_URL.Substring(index + 1);
                                                }
                                                else if (workHead.Contains("Performance")) //rpb
                                                {
                                                    lrd.RPB_URL = workVal;
                                                    index = lrd.RPB_URL.LastIndexOf('/');
                                                    lrd.RPB_SheetID = lrd.RPB_URL.Substring(index + 1);
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(lrd?.LogID))
                                                ActivePost.AddLog(raid, lrd);
                                        }
                                    }

                                    done = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return done;
        }

        public static LoggedRaid GetRaidFromLine(string line)
        {
            LoggedRaid r = LoggedRaid.NONE;

            line = line?.ToUpper()?.Trim() ?? "";

            if (line.StartsWith("GRU + MAG") || (line.Contains("GRU") && line.Contains("MAG")))
                r = LoggedRaid.GRUUL_MAG;
            else if (line.StartsWith("SSC + TK") || (line.Contains("SSC") && line.Contains("TK")))
                r = LoggedRaid.SSC_TK;
            else if (line.StartsWith("HYJ + BT") || (line.Contains("HYJ") && line.Contains("BT")))
                r = LoggedRaid.HYJAL_BT;

            else if (line.StartsWith("KARA"))
                r = LoggedRaid.KARAZHAN;

            else if (line.StartsWith("GRU"))
                r = LoggedRaid.GRUUL;
            else if (line.StartsWith("MAG"))
                r = LoggedRaid.MAG;

            else if (line.StartsWith("SSC") || line.StartsWith("SERPENT"))
                r = LoggedRaid.SSC;
            else if (line.StartsWith("TK") || line.StartsWith("TEMPEST"))
                r = LoggedRaid.TK;

            else if (line.StartsWith("HYJ"))
                r = LoggedRaid.HYJAL;
            else if (line.StartsWith("BT") || line.StartsWith("BLACK"))
                r = LoggedRaid.BT;

            else if (line.StartsWith("ZA") || line.StartsWith("ZUL"))
                r = LoggedRaid.ZA;
            else if (line.StartsWith("SWP") || line.StartsWith("SUNWELL"))
                r = LoggedRaid.SWP;

            return r;
        }

        public static string CreateLineFromRaid(LoggedRaid r, int index, bool isEmbed, bool isRaidSingle)
        {
            string line = "";

            string indexInName = isRaidSingle ? "" : index.ToString();

            switch (r)
            {
                case LoggedRaid.KARAZHAN:
                    line = "KARA" + indexInName + (isEmbed ? "" : ": ");
                    break;

                case LoggedRaid.GRUUL:
                    line = "GRU" + indexInName + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.MAG:
                    line = "MAG" + indexInName + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.GRUUL_MAG:
                    line = "GRU + MAG" + indexInName + (isEmbed ? "" : ": ");
                    break;

                case LoggedRaid.SSC:
                    line = "SSC" + indexInName + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.TK:
                    line = "TK" + indexInName + (isEmbed ? "" : "  : ");
                    break;
                case LoggedRaid.SSC_TK:
                    line = "SSC + TK" + indexInName + (isEmbed ? "" : " : ");
                    break;

                case LoggedRaid.HYJAL:
                    line = "HYJ" + indexInName + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.BT:
                    line = "BT" + indexInName + (isEmbed ? "" : "  : ");
                    break;
                case LoggedRaid.HYJAL_BT:
                    line = "HYJ + BT" + indexInName + (isEmbed ? "" : " : ");
                    break;

                case LoggedRaid.ZA:
                    line = "ZA" + indexInName + (isEmbed ? "" : "  : ");
                    break;

                case LoggedRaid.SWP:
                    line = "SWP" + indexInName + (isEmbed ? "" : " : ");
                    break;
            }
            return line;
        }

        public class LoggedRaidData
        {
            public LoggedRaid Raid { get; set; }

            public string LogID { get; set; }
            public string CLA_URL { get; set; }
            public string RPB_URL { get; set; }
            public string CLA_SheetID { get; set; }
            public string RPB_SheetID { get; set; }

            public LoggedRaidData(LoggedRaid r) { this.Raid = r; }
            public LoggedRaidData(LoggedRaid r, string logID) : this(r)
            {
                this.LogID = logID;
            }
        }
    }
}
