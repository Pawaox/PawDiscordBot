using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.GoogleAPI
{
    public static class SheetHelper
    {
        public static string GetValueFromRange(SheetsService service, string sheetID, string range)
        {
            string result = "";

            SpreadsheetsResource.ValuesResource.GetRequest getRequest = service.Spreadsheets.Values.Get(sheetID, range);
            ValueRange valueRange = getRequest.Execute();

            if (valueRange?.Values != null)
            {
                if (valueRange.Values.Count > 0)
                {
                    result = valueRange.Values[0]?[0]?.ToString() ?? "";
                }
            }

            return result;
        }

        public static List<string> GetValuesFromRange(SheetsService service, string sheetID, string range)
        {
            List<string> result = new List<string>();

            SpreadsheetsResource.ValuesResource.GetRequest getRequest = service.Spreadsheets.Values.Get(sheetID, range);
            ValueRange valueRange = getRequest.Execute();

            if (valueRange?.Values != null)
            {
                if (valueRange.Values.Count > 0)
                {
                    IList<object> cells = valueRange.Values[0];

                    foreach (var cell in cells)
                    {
                        result.Add(cell?.ToString());
                    }
                }
            }

            return result;
        }

        public static IList<IList<object>> GetMatrixFromRange(SheetsService service, string sheetID, string range)
        {
            IList<IList<object>> lst = new List<IList<object>>();

            SpreadsheetsResource.ValuesResource.GetRequest getRequest = service.Spreadsheets.Values.Get(sheetID, range);
            ValueRange valueRange = getRequest.Execute();

            if (valueRange?.Values != null)
            {
                if (valueRange.Values.Count > 0)
                {
                    lst = valueRange.Values;
                }
            }

            return lst;
        }

        public static void SetTextInRange(SheetsService service, string sheetID, string range, string content)
        {
            IList<IList<object>> valueData = new List<IList<object>>();
            valueData.Add(new List<object>());
            valueData[0].Add(content);

            ValueRange valueRange = new ValueRange();
            valueRange.Values = valueData;

            SpreadsheetsResource.ValuesResource.UpdateRequest updateSheetCellRequest = service.Spreadsheets.Values.Update(valueRange, sheetID, range);
            updateSheetCellRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            updateSheetCellRequest.Execute();
        }

        public static void SetRanges(SheetsService service, string sheetID, string range, List<IList<object>> content)
        {
            IList<IList<object>> valueData = new List<IList<object>>();
            foreach (List<object> lst in content)
                valueData.Add(lst);

            ValueRange valueRange = new ValueRange();
            valueRange.Values = valueData;

            SpreadsheetsResource.ValuesResource.UpdateRequest updateSheetCellRequest = service.Spreadsheets.Values.Update(valueRange, sheetID, range);
            updateSheetCellRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            updateSheetCellRequest.Execute();
        }

        public static bool CallScriptFunction(ScriptService service, string function, string scriptID, Func<bool> isDoneCheck)
        {
            ExecutionRequest request = new ExecutionRequest();
            request.Function = function;
            request.DevMode = true;

            ScriptsResource.RunRequest runReq = service.Scripts.Run(request, scriptID);
            try
            {
                Operation op = runReq.Execute();

                if (op.Error != null)
                {
                    StringBuilder err = new StringBuilder();

                    IDictionary<string, object> error = op.Error.Details[0];
                    err.AppendLine($"Script error message: {error["errorMessage"]}");

                    if (error["scriptStackTraceElements"] != null)
                    {
                        err.AppendLine("Script error stacktrace:");
                        foreach (var trace in (JArray)error["scriptStackTraceElements"])
                            err.AppendLine($"\t{trace["function"]}: {trace["lineNumber"]}");
                    }

                    throw new AppsScriptException(err.ToString());
                }
            }
            catch (TaskCanceledException ex)
            {
                //Took longer than 100sec, but this is fine, it keeps going.
            }

            int runs = 0, maxruns = 20, msPerRun = 15000; //Total 5 extra minutes of waiting. Sheet max wait is 6min, 100sec + 5min is more than that.
            bool isDone = false;

            while (runs < maxruns)
            {
                Thread.Sleep(msPerRun);
                isDone = isDoneCheck();

                if (isDone)
                    break;
                runs++;
            }

            return isDone;
        }

        public static CallScriptFunctionWithReturnResult<T> CallScriptFunctionWithReturn<T>(ScriptService service, string function, string scriptID, Func<CallScriptFunctionWithReturnResult<T>> isDoneCheck)
        {
            CallScriptFunctionWithReturnResult<T> result = new CallScriptFunctionWithReturnResult<T>();

            ExecutionRequest request = new ExecutionRequest();
            request.Function = function;
            request.DevMode = true;

            ScriptsResource.RunRequest runReq = service.Scripts.Run(request, scriptID);
            try
            {
                Operation op = runReq.Execute();

                if (op.Error != null)
                {
                    StringBuilder err = new StringBuilder();

                    IDictionary<string, object> error = op.Error.Details[0];
                    err.AppendLine($"Script error message: {error["errorMessage"]}");

                    if (error["scriptStackTraceElements"] != null)
                    {
                        err.AppendLine("Script error stacktrace:");
                        foreach (var trace in (JArray)error["scriptStackTraceElements"])
                            err.AppendLine($"\t{trace["function"]}: {trace["lineNumber"]}");
                    }

                    throw new AppsScriptException(err.ToString());
                }
            }
            catch (TaskCanceledException ex)
            {
                //Took longer than 100sec, but this is fine, it keeps going.
            }

            int runs = 0, maxruns = 20, msPerRun = 15000; //Total 5 extra minutes of waiting. Sheet max wait is 6min, 100sec + 5min is more than that.

            CallScriptFunctionWithReturnResult<T> outputObj = null;
            while (runs < maxruns)
            {
                Thread.Sleep(msPerRun);
                outputObj = isDoneCheck();

                if (outputObj != null && outputObj.IsDone)
                    break;
                runs++;
            }

            if (outputObj != null && outputObj.IsDone)
            {
                result = outputObj;
            }

            return result;
        }

        public class CallScriptFunctionWithReturnResult<T>
        {
            public bool IsDone { get; set; }
            public T Result { get; set; }
        }
    }
}
