using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GnomeParsingBot.GoogleAPI;
using GnomeParsingBot.WarcraftLogs;
using PawDiscordBot;
using PawDiscordBot.Commands;
using PawDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GnomeParsingBot.ActivePost;

namespace GnomeParsingBot.Commands
{
    public class GenerateAwardsCommand : ParameterCommand
    {
        public GenerateAwardsCommand(string prefix) : base(new CommandSettings(prefix + "generateAwards", 3)) { }

        public override bool HandleCommand(PawDiscordBotClient client, SocketUserMessage msg, string[] levels, string[] parameters)
        {
            bool handled = false;

            if (levels.Length != 1)
                return handled;

            if (Settings.ParameterCount > parameters.Length)
                return handled;

            if (msg.Author is SocketGuildUser)
            {
                if (ActivePost.PostID.HasValue)
                {
                    msg.DeleteAsync();

                    string raidInput = parameters[0].ToUpper();
                    int indexWanted = 1;

                    List<LoggedRaidData> raidData = new List<LoggedRaidData>();
                    if (raidInput.Contains(","))
                    {
                        string[] splts = raidInput.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        foreach (string splt in splts)
                        {
                            char lastChar = splt[splt.Length - 1];

                            if (int.TryParse(lastChar.ToString(), out int res))
                                indexWanted = res;

                            LoggedRaid r = ActivePost.GetRaidFromLine(splt);
                            if (ActivePost.RaidLogs.ContainsKey(r))
                            {
                                LoggedRaidData dat = ActivePost.RaidLogs[r][indexWanted];

                                if (!string.IsNullOrEmpty(dat.CLA_SheetID) && !string.IsNullOrEmpty(dat.RPB_SheetID))
                                    raidData.Add(dat);
                                else
                                    throw new PawDiscordBotException(ExceptionType.WARN_USER, "Couldn't find CLA or RPB data for '" + splt + "'");
                            }
                            else
                                throw new PawDiscordBotException(ExceptionType.WARN_USER, "Couldn't find raiddata for '" + splt + "'");
                        }
                    }
                    else
                    {
                        char lastChar = raidInput[raidInput.Length - 1];

                        if (int.TryParse(lastChar.ToString(), out int res))
                            indexWanted = res;

                        LoggedRaid r = ActivePost.GetRaidFromLine(raidInput);
                        if (ActivePost.RaidLogs.ContainsKey(r))
                        {
                            LoggedRaidData dat = ActivePost.RaidLogs[r][indexWanted];

                            if (!string.IsNullOrEmpty(dat.CLA_SheetID) && !string.IsNullOrEmpty(dat.RPB_SheetID))
                                raidData.Add(dat);
                            else
                                throw new PawDiscordBotException(ExceptionType.WARN_USER, "Couldn't find CLA or RPB data for '" + raidInput + "'");
                        }
                        else
                            throw new PawDiscordBotException(ExceptionType.WARN_USER, "Couldn't find raiddata for '" + raidInput + "'");
                    }

                    if (raidData.Count <= 0)
                        throw new PawDiscordBotException(ExceptionType.WARN_USER, "0 logs found for '" + raidInput + "'");

                    ISocketMessageChannel outputChannel = null;

                    string channelInput = "";
                    if (parameters.Length >= 2)
                        channelInput = parameters[1];

                    string[] giveAwardTriggers = new string[] { "yes", "y", "1", "true" };
                    bool giveAwards = giveAwardTriggers.Contains(parameters[2]);

                    if (channelInput.StartsWith("<#"))
                    {
                        channelInput = channelInput.Substring(2, channelInput.Length - 3);
                        if (ulong.TryParse(channelInput, out ulong newChannel))
                        {
                            SocketGuild guild = client.Client.GetGuild(client.AllowedServerChannelCombo.Item1);
                            outputChannel = guild.GetTextChannel(newChannel);
                        }
                    }

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            List<Tuple<LoggedRaid, string>> rLogs = new List<Tuple<LoggedRaid, string>>();
                            HashSet<string> claSheets = new HashSet<string>();
                            HashSet<string> rpbSheets = new HashSet<string>();

                            foreach (var rd in raidData)
                            {
                                rLogs.Add(new Tuple<LoggedRaid, string>(rd.Raid, rd.LogID));

                                claSheets.Add(rd.CLA_SheetID);
                                rpbSheets.Add(rd.RPB_SheetID);
                            }
                            var individualLogIDs = new string[rLogs.Count];
                            for (int i = 0; i < individualLogIDs.Length; i++)
                                individualLogIDs[i] = rLogs[i].Item2;


                            string[] googleScope = { "https://www.googleapis.com/auth/script.external_request", "https://www.googleapis.com/auth/spreadsheets", "https://www.googleapis.com/auth/drive", "https://www.googleapis.com/auth/drive.file" };
                            var googleCredentials = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromFile(StaticData.PATH_GOOGLECRED_OAUTH).Secrets,
                                googleScope, "user", CancellationToken.None, new Google.Apis.Util.Store.FileDataStore(StaticData.PATH_FILEDATASTORE_CREDENTIALS)).Result;

                            var idiots = AwardChecker.GetMostDeaths(individualLogIDs);

                            var pumpers = AwardChecker.GetMostDamageDone(rLogs.ToArray());
                            var saviours = AwardChecker.GetMostHealingDone(individualLogIDs);

                            var engineers = AwardChecker.GetConsumableScore_EngineeringOnly(individualLogIDs);
                            var consumers = AwardChecker.GetConsumableScore_NoEngineering(individualLogIDs);
                            var farmers = AwardChecker.GetMostChickenProcs(individualLogIDs);

                            var drumScore = AwardChecker.GetBestDrumscore(googleCredentials, claSheets.ToArray());
                            var dmgTaken = AwardChecker.GetAvoidableDamageTaken(googleCredentials, rpbSheets.ToArray());

                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("**AWARDS**");

                            sb.Append("#1 Deaths: \t\t\t\t\t\t\t\t\t  ");
                            GenerateAwardUserText(idiots, ref sb);

                            sb.Append("#1 Boss Damage: \t\t\t\t\t\t  ");
                            GenerateAwardUserText(pumpers.BossDamage, ref sb);

                            sb.Append("#1 Trash Damage: \t\t\t\t\t\t");
                            GenerateAwardUserText(pumpers.TrashDamage, ref sb);

                            sb.Append("#1 Healing Done: \t\t\t\t\t\t  ");
                            GenerateAwardUserText(saviours, ref sb);

                            sb.Append("#1 Consumes Used: \t\t\t\t     ");
                            GenerateAwardUserText(consumers, ref sb);

                            sb.Append("#1 Engineering Used: \t\t\t\t   ");
                            GenerateAwardUserText(engineers, ref sb);

                            sb.Append("#1 Drumscore: \t\t\t\t\t\t\t  ");
                            GenerateAwardUserText(drumScore, ref sb);

                            sb.Append("#1 Chicken Procs: \t\t\t\t\t\t ");
                            GenerateAwardUserText(farmers, ref sb);

                            sb.Append("Most Avoidable DMG Taken: \t");
                            GenerateAwardUserText(dmgTaken.MostTaken, ref sb);

                            sb.Append("Least Avoidable DMG Taken: \t");
                            GenerateAwardUserText(dmgTaken.LeastTaken, ref sb);

                            sb.AppendLine();

                            if (outputChannel == null)
                                outputChannel = msg.Channel;

                            outputChannel.SendMessageAsync(sb.ToString());
                            
                            if (giveAwards)
                            {
                                GiveAwards(client, RoleAward.AwardType.DEATHS, idiots);
                                GiveAwards(client, RoleAward.AwardType.BOSSDAMAGE, pumpers.BossDamage);
                                GiveAwards(client, RoleAward.AwardType.TRASHDAMAGE, pumpers.TrashDamage);
                                GiveAwards(client, RoleAward.AwardType.HEALING, saviours);
                                GiveAwards(client, RoleAward.AwardType.CONSUMES, consumers);
                                GiveAwards(client, RoleAward.AwardType.ENGINEERING, engineers);
                                GiveAwards(client, RoleAward.AwardType.DRUMSCORE, drumScore);
                                GiveAwards(client, RoleAward.AwardType.CHICKEN, farmers);
                                GiveAwards(client, RoleAward.AwardType.AVOIDABLEDAMAGETAKEN_MOST, dmgTaken.MostTaken);
                                GiveAwards(client, RoleAward.AwardType.AVOIDABLEDAMAGETAKEN_LEAST, dmgTaken.LeastTaken);
                            }
                        }
                        catch (Exception exc)
                        {
                            msg.Channel.SendMessageAsync(exc.ToString());
                        }
                    });

                    handled = true;
                }
                else
                    throw new PawDiscordBotException(ExceptionType.WARN_USER, "Please create a new post first!");
            }

            return handled;
        }

        private void GiveAwards<T>(PawDiscordBotClient client, RoleAward.AwardType type, IList<Tuple<string, T>> chars)
        {
            ulong serverID = client.AllowedServerChannelCombo.Item1;
            ulong roleID = RoleAward.RoleIDs[type];

            SocketGuild guild = client.Client.GetGuild(serverID);
            SocketRole role = guild.GetRole(roleID);
            foreach (var member in role.Members)
            {
                member.RemoveRoleAsync(roleID);
            }

            foreach (var player in chars)
            {
                ulong? id= GetPlayerID(player.Item1);

                if (id.HasValue)
                {
                    SocketGuildUser sgu = guild.GetUser(id.Value);
                    sgu.AddRoleAsync(roleID);
                }
            }
        }

        private void GenerateAwardUserText<T>(IList<Tuple<string, T>> data, ref StringBuilder sb)
        {
            int count = data.Count;

            if (count == 1)
                sb.AppendLine(GetPlayerMention(data[0].Item1));
            else
            {
                bool isFirst = true;
                foreach (var idiot in data)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(" & ");

                    sb.Append(GetPlayerMention(idiot.Item1));
                }
                sb.AppendLine();
            }
        }

        private string GetPlayerMention(string charName)
        {
            string result = charName;

            charName = charName.ToUpper();
            if (StaticData.CharactersToDiscordID.ContainsKey(charName))
                result = "<@" + StaticData.CharactersToDiscordID[charName] + ">";

            return result;
        }
        private ulong? GetPlayerID(string charName)
        {
            ulong? id = null;

            charName = charName.ToUpper();
            if (StaticData.CharactersToDiscordID.ContainsKey(charName))
                id = StaticData.CharactersToDiscordID[charName];

            return id;
        }
    }
}
