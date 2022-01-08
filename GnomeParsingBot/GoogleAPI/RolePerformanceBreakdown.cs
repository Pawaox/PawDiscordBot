using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Newtonsoft.Json.Linq;
using static GnomeParsingBot.GoogleAPI.SheetHelper;

namespace GnomeParsingBot.GoogleAPI
{
    public class RolePerformanceBreakdown : RPBCLABase
    {
        static string _templateSpreadsheetScriptID = "1Q-lCdLA0ToI6UyoCUvBy4o4SGRzGVW1w_K5WbsfQfoe6rJVlNSNbVgQ0";
        static string _templateSpreadsheetID = "18TJJyDrPaG_iI3rvTd3vU8_iKL-NdmH3ME7aRv1-hlk";
        static string _rangeKey = "Instructions!E9";
        static string _rangeLog = "Instructions!E11";
        static string _rangeGenerateCompleteText = "All!E3";
        static string _rangeRoleCompleteText = "All!E4";

        static string _cellDescriptionForDamageTaken = "Total (partly) avoidable damage taken";

        public static string[] AvailableRoles = new string[] { "---", "Caster", "Healer", "Physical", "Tank" };

        public RolePerformanceBreakdown(UserCredential creds) : base(creds)
        {
        }

        public Dictionary<string, int> GetAvoidableDamageTaken(string spreadsheetID, bool ignoreTanks)
        {
            Dictionary<string, int> damageTaken = new Dictionary<string, int>();

            string[] sheets = new string[0];
            if (ignoreTanks)
                sheets = new string[] { "Caster", "Healer", "Physical" };
            else
                sheets = new string[] { "Caster", "Healer", "Physical", "Tank" };

            string rangeDesc = "";
            string rangeNames = "";
            string rangeDamageTaken = "";

            var sInit = CreateServiceInitializer();

            using (SheetsService service = new SheetsService(sInit))
            {
                foreach (string sheet in sheets)
                {
                    rangeDesc = sheet + "!A1:A";
                    rangeNames = sheet + "!B1:1";

                    IList<IList<object>> lst = SheetHelper.GetMatrixFromRange(service, spreadsheetID, rangeDesc);

                    int rowIndex = 0;
                    bool foundRow = false;
                    while (rowIndex < lst.Count)
                    {
                        IList<object> innerLst = lst[rowIndex];

                        if (innerLst.Count > 0)
                        {
                            string desc = innerLst[0]?.ToString() ?? "";
                            if (desc.Contains(_cellDescriptionForDamageTaken))
                            {
                                foundRow = true;
                                break;
                            }
                        }

                        rowIndex++;
                    }

                    if (foundRow)
                    {
                        rowIndex++;
                        rangeDamageTaken = sheet + "!B" + rowIndex.ToString() + ":" + rowIndex.ToString();

                        List<string> names = SheetHelper.GetValuesFromRange(service, spreadsheetID, rangeNames);
                        List<string> dmgTaken = SheetHelper.GetValuesFromRange(service, spreadsheetID, rangeDamageTaken);

                        for (int i = 0; i < names.Count; i++)
                        {
                            string name = names[i];
                            string dmgText = dmgTaken[i];

                            if (int.TryParse(dmgText, out int damage))
                            {
                                if (!damageTaken.ContainsKey(name))
                                    damageTaken.Add(name, damage);
                                else
                                    damageTaken[name] += damage;
                            }
                        }
                    }
                }

            }

            return damageTaken;
        }

        public void FixRoles()
        {
            string prefixA = "All!", prefixB = "";

            string[] namesToSkip = new string[] { "Druids", "Hunters", "Mages", "Paladins", "Priests", "Rogues", "Shamans", "Warlocks", "Warriors" };

            string[] series = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            int nextPrefixBIndex = 0;

            int playerNameRow = 7, playerRole1Row = 5, playerRole2Row = 6;

            int role1Index = 0, role2Index = 0;
            int columnIndex = 2; //Starts a D, but index has to be 1 lower since foreach increments on start

            var sInit = CreateServiceInitializer();
            using (SheetsService sheetService = new SheetsService(sInit))
            {
                List<string> playerNames = SheetHelper.GetValuesFromRange(sheetService, _templateSpreadsheetID, $"All!D{playerNameRow}:{playerNameRow}");

                List<object> toSetRoles1 = new List<object>();
                List<object> toSetRoles2 = new List<object>();

                string firstRangeRole1 = "";
                string lastRangeRole2 = "";
                foreach (string playerName in playerNames)
                {
                    columnIndex++;

                    if (columnIndex >= series.Length)
                    {
                        prefixB = series[nextPrefixBIndex++];
                        columnIndex = 0;
                    }

                    if (namesToSkip.Contains(playerName))
                    {
                        toSetRoles1.Add("");
                        toSetRoles2.Add("");
                        continue;
                    }

                    if (string.IsNullOrEmpty(firstRangeRole1))
                        firstRangeRole1 = prefixB + series[columnIndex] + playerRole1Row.ToString();
                    lastRangeRole2 = prefixB + series[columnIndex] + playerRole2Row.ToString();

                    var roleDic = StaticData.CharactersToRoles;

                    if (roleDic.ContainsKey(playerName))
                    {
                        Tuple<string, string> charRoles = roleDic[playerName];

                        if (string.IsNullOrEmpty(charRoles.Item1))
                            toSetRoles1.Add(AvailableRoles[0]);
                        else
                            toSetRoles1.Add(charRoles.Item1);

                        if (string.IsNullOrEmpty(charRoles.Item2))
                            toSetRoles2.Add(AvailableRoles[0]);
                        else
                            toSetRoles2.Add(charRoles.Item2);
                    }
                    else
                        throw new RolePerformanceBreakdownException($"Couldn't find '{playerName}' in roles dictionary!");

                    role1Index++;
                    role2Index++;
                }

                string roleRange = prefixA + firstRangeRole1 + ":" + lastRangeRole2;

                List<IList<object>> finalList = new List<IList<object>>();
                finalList.Add(toSetRoles1);
                finalList.Add(toSetRoles2);
                SheetHelper.SetRanges(sheetService, _templateSpreadsheetID, roleRange, finalList);
            }
        }

        public string ExportSheetData()
        {
            string finalUrl = "";

            var sInit = CreateServiceInitializer();

            using (ScriptService service = new ScriptService(sInit))
            {
                CallScriptFunctionWithReturnResult<string> result = SheetHelper.CallScriptFunctionWithReturn(service, "generateRoleSheets", _templateSpreadsheetScriptID, IsRoleInputCompleted);

                if (result.IsDone)
                    finalUrl = result.Result;
            }

            return finalUrl;
        }

        public void GenerateSheetData()
        {
            var sInit = CreateServiceInitializer();

            using (ScriptService service = new ScriptService(sInit))
            {
                bool isDone = SheetHelper.CallScriptFunction(service, "generateAllSheet", _templateSpreadsheetScriptID, IsSheetGenerationCompleted);

                if (isDone)
                {
                    FixRoles();
                }
            }
        }

        public void PrepareSheet(string apiKey, string logID)
        {
            var sInit = CreateServiceInitializer();

            using (SheetsService sheetService = new SheetsService(sInit))
            {
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeKey, apiKey);
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeLog, logID);
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeGenerateCompleteText, " ");
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeRoleCompleteText, " ");
            }
        }

        public bool IsSheetGenerationCompleted()
        {
            bool isCompleted = false;

            var sInit = CreateServiceInitializer();

            using (SheetsService sheetService = new SheetsService(sInit))
            {
                string res = SheetHelper.GetValueFromRange(sheetService, _templateSpreadsheetID, _rangeGenerateCompleteText);
                isCompleted = res.StartsWith("Step 6 is done");
            }

            return isCompleted;
        }

        public CallScriptFunctionWithReturnResult<string> IsRoleInputCompleted()
        {
            CallScriptFunctionWithReturnResult<string> res = new CallScriptFunctionWithReturnResult<string>();

            bool isCompleted = false;

            var sInit = CreateServiceInitializer();

            using (SheetsService sheetService = new SheetsService(sInit))
            {
                string rangeValue = SheetHelper.GetValueFromRange(sheetService, _templateSpreadsheetID, _rangeRoleCompleteText);
                isCompleted = rangeValue.Contains("spreadsheets");
                if (isCompleted)
                {
                    res.IsDone = isCompleted;
                    res.Result = rangeValue;
                }
            }

            return res;
        }
    }
}
