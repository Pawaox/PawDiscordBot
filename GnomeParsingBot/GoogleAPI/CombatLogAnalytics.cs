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
    public class CombatLogAnalytics : RPBCLABase
    {
        static string _templateSpreadsheetScriptID = "1OaebS9u7R33w31rQuu7MokIvGZjvpYhFSH1juNv-NY8-LwU7TJEIcbPL";
        static string _templateSpreadsheetID = "19VELHXy6QgZubJTMlNHbECXUBGsbDeBhzOzMudgFS_s";
        static string _rangeKey = "Instructions!E9";
        static string _rangeLog = "Instructions!E11";
        static string _rangeGenerateCompleteText = "Instructions!B27";
        static string _rangeSelections = "Instructions!F25:F31";

        static string _rangeCompleted_GearIssues = "gear issues!J1";
        static string _rangeCompleted_GearList = "gear listing!D1";
        static string _rangeCompleted_Buffs = "buff consumables!C1";
        static string _rangeCompleted_Drums = "drums!F1";
        static string _rangeCompleted_FightsSSC = "fightsSSC!I1";
        static string _rangeCompleted_FightsTK = "fightsTK!I1";

        public static string[] AvailableRoles = new string[] { "---", "Caster", "Healer", "Physical", "Tank" };

        public CombatLogAnalytics(UserCredential creds) : base(creds)
        {
        }

        public void PrepareSheet(string apiKey, string logID)
        {
            var sInit = CreateServiceInitializer();

            using (SheetsService sheetService = new SheetsService(sInit))
            {
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeKey, apiKey);
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeLog, logID);

                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeGenerateCompleteText, " ");

                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeCompleted_GearIssues, " ");
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeCompleted_GearList, " ");
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeCompleted_Buffs, " ");
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeCompleted_Drums, " ");
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeCompleted_FightsSSC, " ");
                SheetHelper.SetTextInRange(sheetService, _templateSpreadsheetID, _rangeCompleted_FightsTK, " ");

                //Takes more code, looks uglier, but saves API calls
                List<IList<object>> lst = new List<IList<object>>();
                for (int i = 0; i < 7; i++)
                    lst.Add(new List<object>());
                lst[0].Add("yes"); //gear issues
                lst[1].Add("yes"); //gear listing
                lst[2].Add("yes"); //buffs
                lst[3].Add("yes"); //drums
                lst[4].Add("no"); //validate
                lst[5].Add("no"); //shadow res
                lst[6].Add("yes"); //fights

                SheetHelper.SetRanges(sheetService, _templateSpreadsheetID, _rangeSelections, lst);
            }
        }

        public bool PopulateDataSheets()
        {
            var sInit = CreateServiceInitializer();

            bool gearIssues = false, gearList = false, buffs = false, drums = false, fights = false;
            using (ScriptService service = new ScriptService(sInit))
            {
                var tsk1 = Task.Factory.StartNew(() => { return SheetHelper.CallScriptFunction(service, "populateGearIssues", _templateSpreadsheetScriptID, () => IsPopulateDone(_rangeCompleted_GearIssues)); });
                var tsk2 = Task.Factory.StartNew(() => { return SheetHelper.CallScriptFunction(service, "populateGearBreakdown", _templateSpreadsheetScriptID, () => IsPopulateDone(_rangeCompleted_GearList)); });
                var tsk3 = Task.Factory.StartNew(() => { return SheetHelper.CallScriptFunction(service, "populateBuffConsumables", _templateSpreadsheetScriptID, () => IsPopulateDone(_rangeCompleted_Buffs)); });
                var tsk4 = Task.Factory.StartNew(() => { return SheetHelper.CallScriptFunction(service, "populateDrumsEffectiveness", _templateSpreadsheetScriptID, () => IsPopulateDone(_rangeCompleted_Drums)); });
                var tsk5 = Task.Factory.StartNew(() => { return SheetHelper.CallScriptFunction(service, "populateAllFights", _templateSpreadsheetScriptID, () => IsPopulateDone(_rangeCompleted_FightsSSC) && IsPopulateDone(_rangeCompleted_FightsTK)); });
                
                gearIssues = tsk1.Result;
                gearList = tsk2.Result;
                buffs = tsk3.Result;
                drums = tsk4.Result;
                fights = tsk5.Result;
            }

            return gearIssues && gearList && buffs && drums && fights;
        }

        public Dictionary<string, int> GetDrumScores(string spreadsheetID)
        {
            Dictionary<string, int> drumScores = new Dictionary<string, int>();

            string rangeDrums = "drums!B7:I35";

            var sInit = CreateServiceInitializer();

            using (SheetsService service = new SheetsService(sInit))
            {
                IList<IList<object>> lst = SheetHelper.GetMatrixFromRange(service, spreadsheetID, rangeDrums);

                foreach (var innerLst in lst)
                {
                    string name = innerLst[0]?.ToString() ?? "";
                    string scoreText = innerLst[innerLst.Count - 1]?.ToString() ?? "";

                    if (int.TryParse(scoreText, out int score))
                    {
                        if (!drumScores.ContainsKey(name))
                            drumScores.Add(name, score);
                        else
                            drumScores[name] = score;
                    }
                }
            }

            return drumScores;
        }

        public string ExportSheetData()
        {
            string finalUrl = "";

            var sInit = CreateServiceInitializer();

            using (ScriptService service = new ScriptService(sInit))
            {
                CallScriptFunctionWithReturnResult<string> result = SheetHelper.CallScriptFunctionWithReturn(service, "exportSheets", _templateSpreadsheetScriptID, IsExportDone);

                if (result.IsDone)
                    finalUrl = result.Result;
            }

            return finalUrl;
        }

        public bool IsPopulateDone(string range)
        {
            bool isCompleted = false;

            var sInit = CreateServiceInitializer();

            using (SheetsService sheetService = new SheetsService(sInit))
            {
                string res = SheetHelper.GetValueFromRange(sheetService, _templateSpreadsheetID, range);
                isCompleted = (res ?? "").Length > 10;
            }

            return isCompleted;
        }
        
        public CallScriptFunctionWithReturnResult<string> IsExportDone()
        {
            CallScriptFunctionWithReturnResult<string> res = new CallScriptFunctionWithReturnResult<string>();

            bool isCompleted = false;

            var sInit = CreateServiceInitializer();

            using (SheetsService sheetService = new SheetsService(sInit))
            {
                string rangeValue = SheetHelper.GetValueFromRange(sheetService, _templateSpreadsheetID, _rangeGenerateCompleteText);
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
