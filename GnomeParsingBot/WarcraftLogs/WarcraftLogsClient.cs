using GnomeParsingBot.GoogleAPI;
using GnomeParsingBot.WarcraftLogs.DTOs;
using GnomeParsingBot.WarcraftLogs.RequestResponse;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GnomeParsingBot.WarcraftLogs
{
    internal class WarcraftLogsClient : IDisposable
    {
        private string _apiKey;
        private string _baseUrl;

        private HttpClient _client;

        /// <summary>
        /// </summary>
        /// <param name="baseWarcraftLogsUrl">if null or empty, DEFAULT_BASE_URL will be used instead</param>
        /// <param name="apiKeyPath">if null or empty, DEFAULT_API_KEY_PATH will be used instead</param>
        /// <exception cref="FileNotFoundException"></exception>
        public WarcraftLogsClient(string apiKeyPath, string baseWarcraftLogsUrl = null)
        {
            if (string.IsNullOrEmpty(apiKeyPath))
                apiKeyPath = StaticData.PATH_KEY_BASE;
            if (string.IsNullOrEmpty(baseWarcraftLogsUrl))
                baseWarcraftLogsUrl = StaticData.URL_WARCRAFTLOGS;

            _client = new HttpClient();
            if (System.IO.File.Exists(apiKeyPath))
                _apiKey = System.IO.File.ReadAllText(apiKeyPath);
            else
                throw new FileNotFoundException("Couldn't find API Key at " + apiKeyPath);

            _baseUrl = baseWarcraftLogsUrl;
        }

        public GenerateSheetsResult GenerateSheets(UserCredential credentials, string logID)
        {
            GenerateSheetsResult result = new GenerateSheetsResult();
            /*
            result.SpreadSheetCLA_URL = "https://docs.google.com/spreadsheets/d/1AEaJi0cI8ZRmb2HdU08rTn5OK5M4-rdamveUNiR8Vas/edit?usp=drivesdk";
            result.SpreadSheetRPB_URL = "https://docs.google.com/spreadsheets/d/1nhWKDBbLQXZgefL1tQwn_QSUVld2rrFU7OY67viwpKQ/edit?usp=drivesdk";
            */
            CombatLogAnalytics cla = new CombatLogAnalytics(credentials);
            cla.PrepareSheet(_apiKey, logID);
            bool populate = cla.PopulateDataSheets();

            if (populate)
            {
                result.SpreadSheetCLA_URL = cla.ExportSheetData();
            }

            RolePerformanceBreakdown rpb = new RolePerformanceBreakdown(credentials);
            rpb.PrepareSheet(_apiKey, logID);
            rpb.GenerateSheetData();
            rpb.FixRoles();
            result.SpreadSheetRPB_URL = rpb.ExportSheetData();
            
            //Clean up URL and extract sheetID into own property

            int index = result.SpreadSheetCLA_URL.LastIndexOf('/');
            string properURL = result.SpreadSheetCLA_URL.Substring(0, index);
            index = properURL.LastIndexOf('/');
            result.SpreadSheetCLA_SheetID = properURL.Substring(index + 1);
            result.SpreadSheetCLA_URL = properURL + "/";

            index = result.SpreadSheetRPB_URL.LastIndexOf('/');
            properURL = result.SpreadSheetRPB_URL.Substring(0, index);
            index = properURL.LastIndexOf('/');
            result.SpreadSheetRPB_SheetID = properURL.Substring(index + 1);
            result.SpreadSheetRPB_URL = properURL + "/";

            return result;
        }



        public Dictionary<LoggedRaid, List<string>> GetLogs(string guild, string server, Region region, DateTime onDate)
        {
            long from = ((DateTimeOffset)onDate.Date).ToUnixTimeMilliseconds();
            long to = ((DateTimeOffset)onDate.Date.AddDays(1)).ToUnixTimeMilliseconds();

            return GetLogs(guild, server, region, from, to);
        }
        public Dictionary<LoggedRaid, List<string>> GetLogs(string guild, string server, Region region, DateTime startDate, DateTime endDate)
        {
            long from = ((DateTimeOffset)startDate.Date).ToUnixTimeMilliseconds();
            long to = ((DateTimeOffset)endDate.Date).ToUnixTimeMilliseconds();

            return GetLogs(guild, server, region, from, to);
        }
        public Dictionary<LoggedRaid, List<string>> GetLogs(string guild, string server, Region region, long fromUnixStamp, long toUnixStamp)
        {
            Dictionary<LoggedRaid, List<string>> result = new Dictionary<LoggedRaid, List<string>>();

            if (string.IsNullOrEmpty(guild))
                throw new ArgumentNullException("guild");
            if (string.IsNullOrEmpty(server))
                throw new ArgumentNullException("server");
            if (fromUnixStamp <= 0)
                throw new ArgumentNullException("fromUnixStamp");
            if (toUnixStamp <= 0)
                throw new ArgumentNullException("toUnixStamp");

            string path = _baseUrl + $"v1/reports/guild/{guild.Replace(" ", "%20").ToLower()}/{server.ToLower()}/{RegionToString(region)}?start={fromUnixStamp}&end={toUnixStamp}&api_key={_apiKey}";
            HttpResponseMessage msg = _client.GetAsync(path).Result;

            if (msg != null && msg.IsSuccessStatusCode)
            {
                WarcraftLogDTO[] logs = JsonConvert.DeserializeObject<WarcraftLogDTO[]>(msg.Content.ReadAsStringAsync().Result);

                if (logs != null)
                {
                    foreach (WarcraftLogDTO log in logs)
                    {
                        LoggedRaid raid = ZoneToRaid(log.zone, log.title);

                        if (raid == LoggedRaid.NONE)
                        {
                            continue;
                        }
                        if (!result.ContainsKey(raid))
                            result.Add(raid, new List<string>());

                        result[raid].Add(log.id);
                    }
                }
            }

            return result;
        }

        public Dictionary<string, int> GetDeaths(string logID)
        {
            Dictionary<string, int> deathCount = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(logID))
                throw new ArgumentNullException("logID");

            long timestamp = 0;
            string path = "";
            
            while (string.IsNullOrEmpty(path) || timestamp > 0)
            {
                path = _baseUrl + $"v1/report/tables/deaths/{logID}?start={timestamp}&end={99999999}&api_key={_apiKey}";
                HttpResponseMessage msg = _client.GetAsync(path).Result;

                if (msg != null && msg.IsSuccessStatusCode)
                {
                    DeathsDTO deaths = JsonConvert.DeserializeObject<DeathsDTO>(msg.Content.ReadAsStringAsync().Result);

                    if (deaths != null && deaths.entries.Length >= 0)
                    {
                        foreach (DeathsDTO.Entry death in deaths.entries)
                        {
                            if (!deathCount.ContainsKey(death.name))
                                deathCount.Add(death.name, 1);
                            else
                                deathCount[death.name] += 1;
                        }

                        if (deaths.entries.Length > 200)
                            timestamp = deaths.entries[deaths.entries.Length - 1].timestamp + 1;
                        else
                            timestamp = -1;
                    }
                    else
                        timestamp = -1;
                }
            }

            return deathCount;
        }



        public GetDamageDoneResponse GetDamageDone(string logID, LoggedRaid raidForEncounters)
        {
            GetDamageDoneResponse resp = new GetDamageDoneResponse();
            resp.TotalDamage = new Dictionary<string, long>();
            resp.TrashDamage = new Dictionary<string, long>();
            resp.BossDamage = new Dictionary<string, long>();

            if (string.IsNullOrEmpty(logID))
                throw new ArgumentNullException("logID");


            string path = _baseUrl + $"v1/report/tables/damage-done/{logID}?start={0}&end={99999999}&by=source&api_key={_apiKey}";
            HttpResponseMessage msg = _client.GetAsync(path).Result;

            if (msg != null && msg.IsSuccessStatusCode)
            {
                DamageDoneTableDTO dmg = JsonConvert.DeserializeObject<DamageDoneTableDTO>(msg.Content.ReadAsStringAsync().Result);

                if (dmg != null)
                {
                    foreach (DamageDoneTableDTO.Entry entry in dmg.entries)
                    {
                        if (!resp.TotalDamage.ContainsKey(entry.name))
                            resp.TotalDamage.Add(entry.name, entry.total);
                        else
                            resp.TotalDamage[entry.name] += entry.total;
                    }
                }
            }

            int[] encounters = GetEncountersForRaid(raidForEncounters);

            foreach (int enc in encounters)
            {
                path = _baseUrl + $"v1/report/tables/damage-done/{logID}?start={0}&end={99999999}&by=source&api_key={_apiKey}&encounter={enc}";
                msg = _client.GetAsync(path).Result;

                if (msg != null && msg.IsSuccessStatusCode)
                {
                    DamageDoneTableDTO dmg = JsonConvert.DeserializeObject<DamageDoneTableDTO>(msg.Content.ReadAsStringAsync().Result);

                    if (dmg != null)
                    {
                        foreach (DamageDoneTableDTO.Entry entry in dmg.entries)
                        {
                            if (!resp.BossDamage.ContainsKey(entry.name))
                                resp.BossDamage.Add(entry.name, entry.total);
                            else
                                resp.BossDamage[entry.name] += entry.total;
                        }
                    }
                }
            }

            foreach (var pair in resp.TotalDamage)
            {
                if (!resp.TrashDamage.ContainsKey(pair.Key))
                    resp.TrashDamage.Add(pair.Key, pair.Value);
                else
                    resp.TrashDamage[pair.Key] += pair.Value;
            }

            foreach (var pair in resp.BossDamage)
            {
                if (resp.TrashDamage.ContainsKey(pair.Key))
                    resp.TrashDamage[pair.Key] -= pair.Value;
            }

            return resp;
        }



        public Dictionary<string, long> GetHealingDone(string logID)
        {
            Dictionary<string, long> healing = new Dictionary<string, long>();

            if (string.IsNullOrEmpty(logID))
                throw new ArgumentNullException("logID");

            string path = _baseUrl + $"v1/report/tables/healing/{logID}?start={0}&end={99999999}&api_key={_apiKey}";
            HttpResponseMessage msg = _client.GetAsync(path).Result;

            if (msg != null && msg.IsSuccessStatusCode)
            {
                HealingDoneTableDTO heals = JsonConvert.DeserializeObject<HealingDoneTableDTO>(msg.Content.ReadAsStringAsync().Result);

                if (heals != null)
                {
                    foreach (HealingDoneTableDTO.Entry entry in heals.entries)
                    {
                        if (!healing.ContainsKey(entry.name))
                            healing.Add(entry.name, entry.total);
                        else
                            healing[entry.name] += entry.total;
                    }
                }
            }

            return healing;
        }

        public Dictionary<string, long> GetChickenProcs(string logID)
        {
            Dictionary<string, long> chickenProcs = new Dictionary<string, long>();

            Dictionary<int, string> chickenToPlayer = new Dictionary<int, string>();

            if (string.IsNullOrEmpty(logID))
                throw new ArgumentNullException("logID");

            string path = _baseUrl + $"v1/report/tables/damage-done/{logID}?start={0}&end={99999999}&api_key={_apiKey}";
            HttpResponseMessage msg = _client.GetAsync(path).Result;

            #region Map Chickens to players
            if (msg != null && msg.IsSuccessStatusCode)
            {
                DamageDoneTableDTO dmg = JsonConvert.DeserializeObject<DamageDoneTableDTO>(msg.Content.ReadAsStringAsync().Result);

                if (dmg != null)
                {
                    foreach (DamageDoneTableDTO.Entry entry in dmg.entries)
                    {
                        if (entry.pets != null && entry.pets.Length > 0)
                        {
                            foreach (DamageDoneTableDTO.Pet pet in entry.pets)
                            {
                                if ("Battle Chicken".Equals(pet.name))
                                {
                                    if (!chickenToPlayer.ContainsKey(pet.id))
                                        chickenToPlayer.Add(pet.id, entry.name);
                                    else
                                        chickenToPlayer[pet.id] = entry.name;
                                }
                            }
                        }
                    }
                }
            }
            #endregion


            path = _baseUrl + $"v1/report/tables/buffs/{logID}?start={0}&end={99999999}&api_key={_apiKey}&abilityid={23060}&by=target";
            msg = _client.GetAsync(path).Result;

            if (msg != null && msg.IsSuccessStatusCode)
            {
                BuffTablesDTO buffs = JsonConvert.DeserializeObject<BuffTablesDTO>(msg.Content.ReadAsStringAsync().Result);

                if (buffs != null)
                {
                    foreach (BuffTablesDTO.Aura aura in buffs.auras)
                    {
                        if (chickenToPlayer.ContainsKey(aura.id))
                        {
                            if ("Battle Chicken".Equals(aura.name))
                            {
                                string playerName = chickenToPlayer[aura.id];
                                if (!chickenProcs.ContainsKey(playerName))
                                    chickenProcs.Add(playerName, aura.totalUses);
                                else
                                    chickenProcs[playerName] += aura.totalUses;
                            }
                        }
                    }
                }
            }

            return chickenProcs;
        }

        public Tuple<Dictionary<string, long>, Dictionary<string, long>> GetHealerPacifism(string logID, params string[] healerNames)
        {
            Dictionary<string, long> hatredSpread = new Dictionary<string, long>();
            Dictionary<string, long> murderers = new Dictionary<string, long>();

            Dictionary<int, string> healerIdsToCharacter = new Dictionary<int, string>();

            if (string.IsNullOrEmpty(logID))
                throw new ArgumentNullException("logID");

            string path = _baseUrl + $"v1/report/tables/healing/{logID}?start={0}&end={99999999}&api_key={_apiKey}";
            HttpResponseMessage msg = _client.GetAsync(path).Result;

            #region Map Chickens to players
            if (msg != null && msg.IsSuccessStatusCode)
            {
                HealingDoneTableDTO heals = JsonConvert.DeserializeObject<HealingDoneTableDTO>(msg.Content.ReadAsStringAsync().Result);
                
                if (heals != null)
                {
                    foreach (HealingDoneTableDTO.Entry entry in heals.entries)
                    {
                        if (healerNames.Contains(entry?.name ?? ""))
                        {
                            if (!healerIdsToCharacter.ContainsKey(entry.id))
                                healerIdsToCharacter.Add(entry.id, entry.name);
                            else
                                healerIdsToCharacter[entry.id] = entry.name;
                        }
                    }
                }
            }
            #endregion

            foreach (var ch in healerIdsToCharacter.Values)
            {
                if (!murderers.ContainsKey(ch))
                    murderers.Add(ch, 0);
                if (!hatredSpread.ContainsKey(ch))
                    hatredSpread.Add(ch, 0);
            }

            long startValue = 1;
            int apiCallCount = 0;
            while (startValue > 0)
            {
                path = _baseUrl + $"v1/report/events/damage-taken/{logID}?start={startValue}&end={99999999}&api_key={_apiKey}&by=source";
                msg = _client.GetAsync(path).Result;

                if (msg != null && msg.IsSuccessStatusCode)
                {
                    DamageTakenEventsDTO events = JsonConvert.DeserializeObject<DamageTakenEventsDTO>(msg.Content.ReadAsStringAsync().Result);
                    apiCallCount++;

                    if (events?.events != null)
                    {
                        startValue = events.nextPageTimestamp;

                        foreach (DamageTakenEventsDTO.Event ev in events.events)
                        {
                            if (healerIdsToCharacter.ContainsKey(ev.sourceID) && ev.targetIsFriendly && ev.sourceIsFriendly)
                            {
                                string player = healerIdsToCharacter[ev.sourceID];

                                if (ev.overkill > 0)
                                {
                                    if (!murderers.ContainsKey(player))
                                        murderers.Add(player, 1);
                                    else
                                        murderers[player] += 1;
                                }

                                if (!hatredSpread.ContainsKey(player))
                                    hatredSpread.Add(player, ev.unmitigatedAmount > 0 ? ev.unmitigatedAmount : ev.amount > 0 ? ev.amount : 0);
                                else
                                    hatredSpread[player] += ev.unmitigatedAmount > 0 ? ev.unmitigatedAmount : ev.amount > 0 ? ev.amount : 0;
                            }
                        }
                    }
                    else
                        break;
                }
                else
                    break;
            }

            return new Tuple<Dictionary<string, long>, Dictionary<string, long>>(hatredSpread, murderers);
        }

        public Dictionary<string, long> GetConsumableUse(string logID, GetConsumableUseRequest consumable)
        {
            Dictionary<string, long> uses = new Dictionary<string, long>();

            Dictionary<int, string> sourceIDToPlayer = new Dictionary<int, string>();

            if (string.IsNullOrEmpty(logID))
                throw new ArgumentNullException("logID");
            if (consumable == null)
                throw new ArgumentNullException("consumable");
            if (consumable.SpellID <= 0)
                throw new ArgumentOutOfRangeException("spellID cannot be <= 0");

            string path = _baseUrl + $"v1/report/tables/damage-done/{logID}?start={0}&end={99999999}&api_key={_apiKey}";
            HttpResponseMessage msg = _client.GetAsync(path).Result;

            #region Map IDs to players
            if (msg != null && msg.IsSuccessStatusCode)
            {
                DamageDoneTableDTO dmgDoneTable = JsonConvert.DeserializeObject<DamageDoneTableDTO>(msg.Content.ReadAsStringAsync().Result);

                if (dmgDoneTable != null)
                {
                    foreach (DamageDoneTableDTO.Entry entry in dmgDoneTable.entries)
                    {
                        if (!sourceIDToPlayer.ContainsKey(entry.id))
                            sourceIDToPlayer.Add(entry.id, entry.name);
                        else
                            sourceIDToPlayer[entry.id] = entry.name;
                    }
                }
            }
            #endregion

            path = "";
            long timestamp = 0;
            while (string.IsNullOrEmpty(path) || timestamp > 0)
            {
                path = _baseUrl + $"v1/report/events/casts/{logID}?start={timestamp}&end={99999999}&api_key={_apiKey}&abilityid={consumable.SpellID}";
                msg = _client.GetAsync(path).Result;

                if (msg != null && msg.IsSuccessStatusCode)
                {
                    CastEventsDTO casts = JsonConvert.DeserializeObject<CastEventsDTO>(msg.Content.ReadAsStringAsync().Result);

                    if (casts != null)
                    {
                        foreach (CastEventsDTO.Event ev in casts.events)
                        {
                            if ("cast".Equals(ev.type))
                            {
                                if (ev.ability != null/* && consumable.Name.Equals(ev.ability.name)*/)
                                {
                                    if (sourceIDToPlayer.ContainsKey(ev.sourceID))
                                    {
                                        string playerName = sourceIDToPlayer[ev.sourceID];

                                        if (!uses.ContainsKey(playerName))
                                            uses.Add(playerName, 1);
                                        else
                                            uses[playerName] += 1;
                                    }
                                }
                            }
                        }

                        if (casts.events.Length > 0)
                            timestamp = casts.events[casts.events.Length - 1].timestamp + 1;
                        else
                            timestamp = -1;
                    }
                }
            }

            return uses;
        }

        public void Dispose()
        {
            _apiKey = null;
            _baseUrl = null;

            if (_client != null)
                _client.Dispose();
        }

        private int[] GetEncountersForRaid(LoggedRaid r)
        {
            int[] res = new int[0];

            List<int> twoInOne = new List<int>();

            switch (r)
            {
                case LoggedRaid.KARAZHAN:
                    res = new int[] { 652, 653, 654, 655, 656, 657, 658, 659, 661, 662 };
                    break;

                case LoggedRaid.GRUUL:
                    res = new int[] { 649, 650 };
                    break;
                case LoggedRaid.MAG:
                    res = new int[] { 651 };
                    break;


                case LoggedRaid.SSC:
                    res = new int[] { 623, 624, 625, 626, 627, 628 };
                    break;
                case LoggedRaid.TK:
                    res = new int[] { 730, 731, 732, 733 };
                    break;


                case LoggedRaid.HYJAL:
                    res = new int[] { 618, 619, 620, 621, 622 };
                    break;
                case LoggedRaid.BT:
                    res = new int[] { 601, 602, 603, 604, 605, 606, 607, 608, 609 };
                    break;



                case LoggedRaid.GRUUL_MAG:
                    twoInOne.AddRange(GetEncountersForRaid(LoggedRaid.GRUUL));
                    twoInOne.AddRange(GetEncountersForRaid(LoggedRaid.MAG));
                    res = twoInOne.ToArray();
                    break;

                case LoggedRaid.SSC_TK:
                    twoInOne.AddRange(GetEncountersForRaid(LoggedRaid.SSC));
                    twoInOne.AddRange(GetEncountersForRaid(LoggedRaid.TK));
                    res = twoInOne.ToArray();
                    break;

                case LoggedRaid.HYJAL_BT:
                    twoInOne.AddRange(GetEncountersForRaid(LoggedRaid.HYJAL));
                    twoInOne.AddRange(GetEncountersForRaid(LoggedRaid.BT));
                    res = twoInOne.ToArray();
                    break;

                default:
                    throw new NotImplementedException("WarcraftLoggsClient.GetEncountersForRaid(), LoggedRaid = '" + r.ToString() + "'");
            }

            return res;
        }

        private LoggedRaid ZoneToRaid(int zone, string title)
        {
            LoggedRaid r = LoggedRaid.NONE;
            string titleToUse = title?.Trim()?.ToUpper() ?? "";

            bool raid1 = false, raid2 = false;
            switch (zone)
            {
                case 1007:
                    r = LoggedRaid.KARAZHAN;
                    break;
                case 1008: //GRUUL & MAG
                    raid1 = titleToUse.Contains("GRU");
                    raid2 = titleToUse.Contains("MAG");

                    if (raid1 && raid2)
                        r = LoggedRaid.GRUUL_MAG;
                    else if (raid1)
                        r = LoggedRaid.GRUUL;
                    else if (raid2)
                        r = LoggedRaid.MAG;
                    break;
                case 1010: //SSC / TK
                    raid1 = titleToUse.Contains("SSC");
                    raid2 = titleToUse.Contains("TK");

                    if (raid1 && raid2)
                        r = LoggedRaid.SSC_TK;
                    else if (raid1)
                        r = LoggedRaid.SSC;
                    else if (raid2)
                        r = LoggedRaid.TK;
                    break;
                case 1011: // BT / Hyjal
                    raid1 = titleToUse.Contains("HYJ");
                    raid2 = titleToUse.Contains("BT") || titleToUse.Contains("BLACK") || titleToUse.Contains("TEMPLE");

                    if (raid1 && raid2)
                        r = LoggedRaid.HYJAL_BT;
                    else if (raid1)
                        r = LoggedRaid.HYJAL;
                    else if (raid2)
                        r = LoggedRaid.BT;
                    break;
                case 1012: // Guessing ZA?
                    r = LoggedRaid.ZA;
                    break;
                case 1013: // Guessing SWP?
                    r = LoggedRaid.SWP;
                    break;
            }
            return r;
        }

        private string RegionToString(Region region)
        {
            string s = "";

            switch (region)
            {
                case Region.EU:
                    s = "eu";
                    break;
                case Region.US:
                    s = "us";
                    break;
            }
            return s;
        }
        internal enum Region
        {
            EU,
            US
        }

        public class GenerateSheetsResult
        {
            public string SpreadSheetCLA_SheetID { get; set; }
            public string SpreadSheetRPB_SheetID { get; set; }
            public string SpreadSheetCLA_URL { get; set; }
            public string SpreadSheetRPB_URL { get; set; }
        }
    }
}
