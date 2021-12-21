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
        public static Dictionary<LoggedRaid, Dictionary<int, string>> RaidLogs { get; set; }

        static ActivePost()
        {
            DateLine = "";
            RaidLogs = new Dictionary<LoggedRaid, Dictionary<int, string>>();
        }

        public static void AddLog(LoggedRaid raid, string log)
        {
            if (!RaidLogs.ContainsKey(raid))
                RaidLogs.Add(raid, new Dictionary<int, string>());

            int raidCount = RaidLogs[raid].Count;

            RaidLogs[raid].Add(raidCount + 1, log);
        }

        public static void EditLog(LoggedRaid raid, int index, string log)
        {
            if (RaidLogs.ContainsKey(raid))
            {
                if (RaidLogs[raid].ContainsKey(index))
                {
                    RaidLogs[raid][index] = log;
                }
            }
        }

        public static async void RewriteMessage(PawDiscordBotClient client, ISocketMessageChannel channel)
        {
            if (!PostID.HasValue)
                return;

            Embed embed = GenerateEmbed();

            await channel.ModifyMessageAsync(PostID.Value, (prop) =>
            {
                prop.Content = "";
                prop.Embed = embed;
            });
        }

        public static Embed GenerateEmbed()
        {/*
            EmbedBuilder b = new EmbedBuilder();
            b.WithTitle("Title Test");
            b.WithDescription("Some description test stuff");
            b.WithColor(new Color(0, 255, 0));
            b.WithCurrentTimestamp();
            b.AddField("SSC1", "[WarcraftLogs](https://www.google.com) - [Combat Analytics](https://www.google.com) - [Role Performance](https://www.google.com)", false);
            b.AddField("SSC2", "[Logs](https://www.google.com) - [Analytics](https://www.google.com) - [Performance](https://www.google.com)", false);
            */

            EmbedBuilder b = new EmbedBuilder();
            b.WithTitle(DateLine);
            
            bool isFirst = true;
            if (RaidLogs.ContainsKey(LoggedRaid.SWP))
            {
                foreach (var v in RaidLogs[LoggedRaid.SWP])
                {
                    string field = CreateLineFromRaid(LoggedRaid.SWP, v.Key, true);
                    string wcl = StaticData.URL_WARCRAFTLOGS_REPORTS + v.Value + "/";
                    string analytics = "";
                    string performance = "";

                    b.AddField(field, "[WarcraftLogs](" + wcl + ") - " + "[Combat Analytics](" + analytics + ") - " + "[Role Performance](" + performance + ")");
                }
                isFirst = false;
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
                    sb.Append(CreateLineFromRaid(r, index, false)).AppendLine("<" + StaticData.URL_WARCRAFTLOGS_REPORTS + RaidLogs[r][index] + "/>");
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

        public static void FetchFromWarcraftlogs(ulong id, string firstLine, DateTime dateToFetch)
        {
            if (!StaticData.Initialized)
                throw new PawDiscordBotException(ExceptionType.WARN_USER, "Bot not properly configured, StaticData is not initialized!");

            ResetMessage(id, firstLine);

            using (WarcraftLogsClient logClient = new WarcraftLogsClient())
            {
                var dict = logClient.GetLogs(StaticData.WarcraftLogsGuildName, StaticData.WarcraftLogsServer, StaticData.WarcraftLogsRegion, dateToFetch);

                foreach (var keyPair in dict)
                {
                    if (!RaidLogs.ContainsKey(keyPair.Key))
                        RaidLogs.Add(keyPair.Key, new Dictionary<int, string>());

                    int count = 1;
                    foreach (string log in keyPair.Value)
                    {
                        RaidLogs[keyPair.Key].Add(count++, log);
                    }
                }
            }
        }

        private static void ClearData()
        {
            RaidLogs = new Dictionary<LoggedRaid, Dictionary<int, string>>();
        }




        public static LoggedRaid GetRaidFromLine(string line)
        {
            LoggedRaid r = LoggedRaid.NONE;

            line = line?.ToUpper()?.Trim() ?? "";

            if (line.StartsWith("GRU + MAG"))
                r = LoggedRaid.GRUUL_MAG;
            else if (line.StartsWith("SSC + TK"))
                r = LoggedRaid.SSC_TK;
            else if (line.StartsWith("HYJ + BT"))
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

        public static string CreateLineFromRaid(LoggedRaid r, int index, bool isEmbed)
        {
            string line = "";

            switch (r)
            {
                case LoggedRaid.KARAZHAN:
                    line = "KARA" + index.ToString() + (isEmbed ? "" : ": ");
                    break;

                case LoggedRaid.GRUUL:
                    line = "GRU" + index.ToString() + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.MAG:
                    line = "MAG" + index.ToString() + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.GRUUL_MAG:
                    line = "GRU + MAG" + index.ToString() + (isEmbed ? "" : ": ");
                    break;

                case LoggedRaid.SSC:
                    line = "SSC" + index.ToString() + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.TK:
                    line = "TK" + index.ToString() + (isEmbed ? "" : "  : ");
                    break;
                case LoggedRaid.SSC_TK:
                    line = "SSC + TK" + index.ToString() + (isEmbed ? "" : " : ");
                    break;

                case LoggedRaid.HYJAL:
                    line = "HYJ" + index.ToString() + (isEmbed ? "" : " : ");
                    break;
                case LoggedRaid.BT:
                    line = "BT" + index.ToString() + (isEmbed ? "" : "  : ");
                    break;
                case LoggedRaid.HYJAL_BT:
                    line = "HYJ + BT" + index.ToString() + (isEmbed ? "" : " : ");
                    break;

                case LoggedRaid.ZA:
                    line = "ZA" + index.ToString() + (isEmbed ? "" : "  : ");
                    break;

                case LoggedRaid.SWP:
                    line = "SWP" + index.ToString() + (isEmbed ? "" : " : ");
                    break;
            }
            return line;
        }
    }
}
