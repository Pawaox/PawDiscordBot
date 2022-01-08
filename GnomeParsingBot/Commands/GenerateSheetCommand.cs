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
    public class GenerateSheetCommand : ParameterCommand
    {
        public GenerateSheetCommand(string prefix) : base(new CommandSettings(prefix + "generateSheets", 1)) { }

        public override bool HandleCommand(PawDiscordBotClient client, SocketUserMessage msg, string[] levels, string[] parameters)
        {
            bool handled = false;

            if (levels.Length != 1)
                return handled;

            if (Settings.ParameterCount != parameters.Length)
                return handled;

            if (msg.Author is SocketGuildUser)
            {
                //SocketGuildUser socketUser = (SocketGuildUser)msg.Author;

                if (ActivePost.PostID.HasValue)
                {
                    msg.DeleteAsync();

                    string input = parameters[0].ToUpper();
                    char lastChar = input[input.Length - 1];
                    int indexWanted = 1;

                    if (int.TryParse(lastChar.ToString(), out int res))
                        indexWanted = res;

                    LoggedRaid r = ActivePost.GetRaidFromLine(input);

                    if (ActivePost.RaidLogs.ContainsKey(r))
                    {
                        LoggedRaidData raidData = ActivePost.RaidLogs[r][indexWanted];

                        Task.Factory.StartNew(() =>
                        {
                            string finalMessage = "";
                            try
                            {
                                string statusMessage = "**[" + DateTime.Now.ToString("HH:mm:ss") + "]:** *Generating " + r.ToString() + " RPB & CLA sheets... This will take 8-12 minutes*";
                                ActivePost.RewriteMessage(statusMessage, client, msg.Channel);

                                string[] googleScope = { "https://www.googleapis.com/auth/script.external_request", "https://www.googleapis.com/auth/spreadsheets", "https://www.googleapis.com/auth/drive", "https://www.googleapis.com/auth/drive.file" };
                                var googleCredentials = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromFile(StaticData.PATH_GOOGLECRED_OAUTH).Secrets,
                                    googleScope, "user", CancellationToken.None, new Google.Apis.Util.Store.FileDataStore(StaticData.PATH_FILEDATASTORE_CREDENTIALS)).Result;

                                /*
                                raidData.CLA_URL = "https://docs.google.com/spreadsheets/d/1vYqTvOPdRYVubiBBmuRAYCK4JJX7-K-v8rAAeXyGhgY/";
                                raidData.RPB_URL = "https://docs.google.com/spreadsheets/d/1uN2TyqyR6PwxiS8wjilAJWiibNhPKHIzCgyVaPNtdyY/";
                                raidData.CLA_SheetID = "1vYqTvOPdRYVubiBBmuRAYCK4JJX7-K-v8rAAeXyGhgY";
                                raidData.RPB_SheetID = "1uN2TyqyR6PwxiS8wjilAJWiibNhPKHIzCgyVaPNtdyY";
                                */
                                using (WarcraftLogsClient wcl = new WarcraftLogsClient(null))
                                {
                                    WarcraftLogsClient.GenerateSheetsResult generatedSheets = wcl.GenerateSheets(googleCredentials, raidData.LogID);
                                    raidData.CLA_URL = generatedSheets.SpreadSheetCLA_URL;
                                    raidData.RPB_URL = generatedSheets.SpreadSheetRPB_URL;
                                    raidData.CLA_SheetID = generatedSheets.SpreadSheetCLA_SheetID;
                                    raidData.RPB_SheetID = generatedSheets.SpreadSheetRPB_SheetID;
                                }

                                ActivePost.EditLog(r, indexWanted, raidData);
                            }
                            catch (Exception exc)
                            {
                                finalMessage = exc.ToString();
                            }

                            ActivePost.RewriteMessage(finalMessage, client, msg.Channel);
                        });
                    }
                    else
                    {
                        throw new PawDiscordBotException(ExceptionType.WARN_USER, "Couldn't find a raid from '" + input + "'");
                    }

                    handled = true;
                }
                else
                    throw new PawDiscordBotException(ExceptionType.WARN_USER, "Please create a new post first!");
            }

            return handled;
        }
    }
}
