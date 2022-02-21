using GnomeParsingBot.GoogleAPI;
using GnomeParsingBot.WarcraftLogs;
using GnomeParsingBot.WarcraftLogs.RequestResponse;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot
{
    internal static class AwardChecker
    {
        public static List<Tuple<string, int>> GetMostDeaths(params string[] logIDs)
        {
            Dictionary<string, int> mostDeaths = new Dictionary<string, int>();

            using (WarcraftLogsClient wcl = new WarcraftLogsClient(StaticData.PATH_KEY_AWARDS))
            {
                foreach (string logID in logIDs)
                {
                    var logDeaths = wcl.GetDeaths(logID);
                    foreach (var d in logDeaths)
                    {
                        if (!mostDeaths.ContainsKey(d.Key))
                            mostDeaths.Add(d.Key, d.Value);
                        else
                            mostDeaths[d.Key] += d.Value;
                    }
                }
            }

            return GetGreatestValue(mostDeaths);
        }


        public static List<Tuple<string, int>> GetBestDrumscore(UserCredential credentials, params string[] claSpreadSheetIDs)
        {
            Dictionary<string, int> combinedScore = new Dictionary<string, int>();

            CombatLogAnalytics cla = new CombatLogAnalytics(credentials);
            foreach (string spreadSheetID in claSpreadSheetIDs)
            {
                var dicDrumScores = cla.GetDrumScores(spreadSheetID);

                foreach (var pair in dicDrumScores)
                {
                    if (!combinedScore.ContainsKey(pair.Key))
                        combinedScore.Add(pair.Key, pair.Value);
                    else
                        combinedScore[pair.Key] += pair.Value;
                }
            }

            return GetGreatestValue(combinedScore);
        }
        
        public static GetAvoidableDamageTakenResult GetAvoidableDamageTaken(UserCredential credentials, params string[] rpbSpreadSheetIDs)
        {
            GetAvoidableDamageTakenResult result = new GetAvoidableDamageTakenResult();

            Dictionary<string, int> combinedScore = new Dictionary<string, int>();

            RolePerformanceBreakdown rpb = new RolePerformanceBreakdown(credentials);
            foreach (string spreadSheetID in rpbSpreadSheetIDs)
            {
                var dicDrumScores = rpb.GetAvoidableDamageTaken(spreadSheetID, true);

                foreach (var pair in dicDrumScores)
                {
                    if (!combinedScore.ContainsKey(pair.Key))
                        combinedScore.Add(pair.Key, pair.Value);
                    else
                        combinedScore[pair.Key] += pair.Value;
                }
            }
            int currentValueLeast = int.MaxValue, currentValueMax = -1;

            foreach (var pair in combinedScore)
            {
                if (pair.Value > currentValueMax)
                {
                    result.MostTaken.Clear();
                    result.MostTaken.Add(new Tuple<string, int>(pair.Key, pair.Value));
                    currentValueMax = pair.Value;
                }
                else if (pair.Value == currentValueMax)
                {
                    result.MostTaken.Add(new Tuple<string, int>(pair.Key, pair.Value));
                }

                if (pair.Value < currentValueLeast)
                {
                    result.LeastTaken.Clear();
                    result.LeastTaken.Add(new Tuple<string, int>(pair.Key, pair.Value));
                    currentValueLeast = pair.Value;
                }
                else if (pair.Value == currentValueLeast)
                {
                    result.LeastTaken.Add(new Tuple<string, int>(pair.Key, pair.Value));
                }
            }

            return result;
        }


        public static GetMostDamageDoneResult GetMostDamageDone(params Tuple<LoggedRaid, string>[] logIDs)
        {
            GetMostDamageDoneResult result = new GetMostDamageDoneResult();

            List<Tuple<string, long>> bestPlayers = new List<Tuple<string, long>>();

            using (WarcraftLogsClient wcl = new WarcraftLogsClient(StaticData.PATH_KEY_AWARDS))
            {
                Dictionary<string, long> bossDam = new Dictionary<string, long>();
                Dictionary<string, long> trashDam = new Dictionary<string, long>();
                Dictionary<string, long> totalDam = new Dictionary<string, long>();

                foreach (var logPair in logIDs)
                {
                    var logDamage = wcl.GetDamageDone(logPair.Item2, logPair.Item1);

                    foreach (var d in logDamage.TotalDamage)
                    {
                        if (!totalDam.ContainsKey(d.Key))
                            totalDam.Add(d.Key, d.Value);
                        else
                            totalDam[d.Key] += d.Value;
                    }
                    foreach (var d in logDamage.TrashDamage)
                    {
                        if (!trashDam.ContainsKey(d.Key))
                            trashDam.Add(d.Key, d.Value);
                        else
                            trashDam[d.Key] += d.Value;
                    }
                    foreach (var d in logDamage.BossDamage)
                    {
                        if (!bossDam.ContainsKey(d.Key))
                            bossDam.Add(d.Key, d.Value);
                        else
                            bossDam[d.Key] += d.Value;
                    }
                }

                result.BossDamage = GetGreatestValue(bossDam);
                result.TrashDamage = GetGreatestValue(trashDam);
                result.TotalDamage = GetGreatestValue(totalDam);
            }

            return result;
        }

        public static List<Tuple<string, long>> GetHealerPacifism(params string[] logIDs)
        {
            Dictionary<string, long> friendlyFire = new Dictionary<string, long>();

            HashSet<string> healers = new HashSet<string>();
            foreach (var charRole in StaticData.CharactersToRoles)
            {
                if ("Healer".Equals(charRole.Value.Item1) || "Healer".Equals(charRole.Value.Item2))
                {
                    healers.Add(charRole.Key);
                }
            }

            using (WarcraftLogsClient wcl = new WarcraftLogsClient(StaticData.PATH_KEY_AWARDS))
            {
                foreach (string logID in logIDs)
                {
                    var logPacifism = wcl.GetHealerPacifism(logID, healers.ToArray());

                    var leastMuderous = GetSmallestValue(logPacifism.Item2);

                    //Incase some of the healers killed people, loop aagain and only take those with least teamkills
                    foreach (var pairFriendlyFire in leastMuderous)
                    {
                        if (logPacifism.Item1.ContainsKey(pairFriendlyFire.Item1))
                        {
                            long ffDam = logPacifism.Item1[pairFriendlyFire.Item1];

                            if (!friendlyFire.ContainsKey(pairFriendlyFire.Item1))
                                friendlyFire.Add(pairFriendlyFire.Item1, ffDam);
                            else
                                friendlyFire[pairFriendlyFire.Item1] += ffDam;
                        }
                    }
                }
            }

            return GetSmallestValue(friendlyFire);
        }

        public static List<Tuple<string, long>> GetMostHealingDone(params string[] logIDs)
        {
            Dictionary<string, long> mostHealing = new Dictionary<string, long>();

            using (WarcraftLogsClient wcl = new WarcraftLogsClient(StaticData.PATH_KEY_AWARDS))
            {
                foreach (string logID in logIDs)
                {
                    var logHeals = wcl.GetHealingDone(logID);
                    foreach (var d in logHeals)
                    {

                        if (!mostHealing.ContainsKey(d.Key))
                            mostHealing.Add(d.Key, d.Value);
                        else
                            mostHealing[d.Key] += d.Value;
                    }
                }
            }

            return GetGreatestValue(mostHealing);
        }

        public static List<Tuple<string, long>> GetMostChickenProcs(params string[] logIDs)
        {
            Dictionary<string, long> mostClucks = new Dictionary<string, long>();

            using (WarcraftLogsClient wcl = new WarcraftLogsClient(StaticData.PATH_KEY_AWARDS))
            {
                foreach (string logID in logIDs)
                {
                    var logClucks = wcl.GetChickenProcs(logID);
                    foreach (var d in logClucks)
                    {
                        if (!mostClucks.ContainsKey(d.Key))
                            mostClucks.Add(d.Key, d.Value);
                        else
                            mostClucks[d.Key] += d.Value;
                    }
                }
            }

            return GetGreatestValue(mostClucks);
        }

        public static List<Tuple<string, float>> GetConsumableScore_NoEngineering(params string[] logIDs)
        {
            GetConsumableUseRequest[] consumes = new GetConsumableUseRequest[]
            {
                new GetConsumableUseRequest() { Name = "Super Mana Potion", SpellID = 28499, Value = 0.5f },
                new GetConsumableUseRequest() { Name = "Cenarion Mana Salve", SpellID = 41617, Value = 0.5f },
                new GetConsumableUseRequest() { Name = "Bottled Nethergon Energy", SpellID = 41618, Value = 0.5f },

                new GetConsumableUseRequest() { Name = "Scroll of Agility IV", SpellID = 12174, Value = 0.5f },
                new GetConsumableUseRequest() { Name = "Scroll of Strength IV", SpellID = 12179, Value = 0.5f },

                new GetConsumableUseRequest() { Name = "Scroll of Protection V", SpellID = 33079, Value = 0.5f },

                new GetConsumableUseRequest() { Name = "Ironshield Potion", SpellID = 28515, Value = 0.75f },

                new GetConsumableUseRequest() { Name = "Scroll of Agility V", SpellID = 33077, Value = 1f },
                new GetConsumableUseRequest() { Name = "Scroll of Strength V", SpellID = 33082, Value = 1f },
                new GetConsumableUseRequest() { Name = "Destruction Potion", SpellID = 28508, Value = 1f },
                new GetConsumableUseRequest() { Name = "Haste Potion", SpellID = 28507, Value = 1f },

                new GetConsumableUseRequest() { Name = "Fel Mana Potion", SpellID = 38929, Value = 1f },
                new GetConsumableUseRequest() { Name = "Flame Cap", SpellID = 28714, Value = 1f },
                new GetConsumableUseRequest() { Name = "Dark Rune", SpellID = 27869, Value = 1f },
                new GetConsumableUseRequest() { Name = "Demonic Rune", SpellID = 16666, Value = 1f },

                new GetConsumableUseRequest() { Name = "Thistle Tea", SpellID = 9512, Value = 1f },
                new GetConsumableUseRequest() { Name = "Mighty Rage Potion", SpellID = 17528, Value = 1f }
            };

            return GetConsumableScore(consumes, logIDs);
        }

        public static List<Tuple<string, float>> GetConsumableScore_EngineeringOnly(params string[] logIDs)
        {
            GetConsumableUseRequest[] consumes = new GetConsumableUseRequest[]
            {
                new GetConsumableUseRequest() { Name = "Fel Iron Bomb", SpellID = 30216, Value = 0.5f },
                new GetConsumableUseRequest() { Name = "Super Sapper Charge", SpellID = 30486, Value = 1f },
                new GetConsumableUseRequest() { Name = "Goblin Sapper Charge", SpellID = 13241, Value = 1f },
                new GetConsumableUseRequest() { Name = "Adamantite Grenade", SpellID = 30217, Value = 1f }
            };

            return GetConsumableScore(consumes, logIDs);
        }

        public static List<Tuple<string, float>> GetConsumableScore(GetConsumableUseRequest[] consumes, params string[] logIDs)
        {
            Dictionary<string, float> mostUses = new Dictionary<string, float>();

            using (WarcraftLogsClient wcl = new WarcraftLogsClient(StaticData.PATH_KEY_AWARDS))
            {
                foreach (string logID in logIDs)
                {
                    foreach (GetConsumableUseRequest consume in consumes)
                    {
                        var logUses = wcl.GetConsumableUse(logID, consume);
                        foreach (var use in logUses)
                        {
                            if (!mostUses.ContainsKey(use.Key))
                                mostUses.Add(use.Key, use.Value * consume.Value);
                            else
                                mostUses[use.Key] += use.Value * consume.Value;
                        }
                    }
                }
            }

            return GetGreatestValue(mostUses);
        }

        #region GetGreatestValue & GetSmallestValue from lists

        #region long versions
        private static List<Tuple<string, long>> GetGreatestValue(Dictionary<string, long> data)
        {
            List<Tuple<string, long>> result = new List<Tuple<string, long>>();

            long currentMax = -1;
            foreach (var pair in data)
            {
                if (pair.Value > currentMax)
                {
                    result.Clear();
                    result.Add(new Tuple<string, long>(pair.Key, pair.Value));
                    currentMax = pair.Value;
                }
                else if (pair.Value == currentMax)
                {
                    result.Add(new Tuple<string, long>(pair.Key, pair.Value));
                }
            }

            return result;
        }

        private static List<Tuple<string, long>> GetSmallestValue(Dictionary<string, long> data)
        {
            List<Tuple<string, long>> result = new List<Tuple<string, long>>();

            long currentMin = long.MaxValue;
            foreach (var pair in data)
            {
                if (pair.Value < currentMin)
                {
                    result.Clear();
                    result.Add(new Tuple<string, long>(pair.Key, pair.Value));
                    currentMin = pair.Value;
                }
                else if (pair.Value == currentMin)
                {
                    result.Add(new Tuple<string, long>(pair.Key, pair.Value));
                }
            }

            return result;
        }
        #endregion
        #region int versions
        private static List<Tuple<string, int>> GetGreatestValue(Dictionary<string, int> data)
        {
            List<Tuple<string, int>> result = new List<Tuple<string, int>>();

            long currentMax = -1;
            foreach (var pair in data)
            {
                if (pair.Value > currentMax)
                {
                    result.Clear();
                    result.Add(new Tuple<string, int>(pair.Key, pair.Value));
                    currentMax = pair.Value;
                }
                else if (pair.Value == currentMax)
                {
                    result.Add(new Tuple<string, int>(pair.Key, pair.Value));
                }
            }

            return result;
        }

        private static List<Tuple<string, int>> GetSmallestValue(Dictionary<string, int> data)
        {
            List<Tuple<string, int>> result = new List<Tuple<string, int>>();

            int currentMin = int.MaxValue;
            foreach (var pair in data)
            {
                if (pair.Value < currentMin)
                {
                    result.Clear();
                    result.Add(new Tuple<string, int>(pair.Key, pair.Value));
                    currentMin = pair.Value;
                }
                else if (pair.Value == currentMin)
                {
                    result.Add(new Tuple<string, int>(pair.Key, pair.Value));
                }
            }

            return result;
        }
        #endregion
        #region float versions
        private static List<Tuple<string, float>> GetGreatestValue(Dictionary<string, float> data)
        {
            List<Tuple<string, float>> result = new List<Tuple<string, float>>();

            float currentMax = -1;
            foreach (var pair in data)
            {
                if (pair.Value > currentMax)
                {
                    result.Clear();
                    result.Add(new Tuple<string, float>(pair.Key, pair.Value));
                    currentMax = pair.Value;
                }
                else if (pair.Value == currentMax)
                {
                    result.Add(new Tuple<string, float>(pair.Key, pair.Value));
                }
            }

            return result;
        }

        private static List<Tuple<string, float>> GetSmallestValue(Dictionary<string, float> data)
        {
            List<Tuple<string, float>> result = new List<Tuple<string, float>>();

            float currentMin = float.MaxValue;
            foreach (var pair in data)
            {
                if (pair.Value < currentMin)
                {
                    result.Clear();
                    result.Add(new Tuple<string, float>(pair.Key, pair.Value));
                    currentMin = pair.Value;
                }
                else if (pair.Value == currentMin)
                {
                    result.Add(new Tuple<string, float>(pair.Key, pair.Value));
                }
            }

            return result;
        }
        #endregion
        
        #endregion


        public class GetMostDamageDoneResult
        {
            public List<Tuple<string, long>> BossDamage { get; set; }
            public List<Tuple<string, long>> TrashDamage { get; set; }
            public List<Tuple<string, long>> TotalDamage { get; set; }

            public GetMostDamageDoneResult()
            {
                BossDamage = new List<Tuple<string, long>>();
                TrashDamage = new List<Tuple<string, long>>();
                TotalDamage = new List<Tuple<string, long>>();
            }
        }

        public class GetAvoidableDamageTakenResult
        {
            public List<Tuple<string, int>> LeastTaken { get; set; }
            public List<Tuple<string, int>> MostTaken { get; set; }

            public GetAvoidableDamageTakenResult()
            {
                LeastTaken = new List<Tuple<string, int>>();
                MostTaken = new List<Tuple<string, int>>();
            }
        }
    }
}
