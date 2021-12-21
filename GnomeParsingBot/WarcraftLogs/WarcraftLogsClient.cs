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
        const string BASE_URL = "https://classic.warcraftlogs.com:443/";
        const string API_KEY_PATH = "C://warcraftlogs_apiKey.txt";

        private string _apiKey;

        private HttpClient _client;

        /// <summary>
        /// Requires the WarcraftLogs API Key to be located at API_KEY_PATH
        /// </summary>
        public WarcraftLogsClient()
        {
            _client = new HttpClient();
            if (System.IO.File.Exists(API_KEY_PATH))
                _apiKey = System.IO.File.ReadAllText(API_KEY_PATH);
            else
                throw new FileNotFoundException("Couldn't find API Key at " + API_KEY_PATH);
        }

        public void Test()
        {
            string[] scope = { SheetsService.Scope.Spreadsheets };
            GoogleCredential sheetCredentials = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("C://gnomeparsing_google_credentials_serviceUser.json").CreateScoped(scope);

            // Create Google Sheets API service.
            var sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = sheetCredentials,
                ApplicationName = "GnomeParsing - RPB and CLA",
            });

            // Define request parameters.
            string spreadsheetId = ADD;
            string rangeApiKey = "Instructions!E9";
            string rangeLogId = "Instructions!E11";

            IList<IList<object>> valueData = new List<IList<object>>();
            valueData.Add(new List<object>());
            valueData[0].Add(_apiKey);

            ValueRange valueRange = new ValueRange();
            valueRange.Values = valueData;

            SpreadsheetsResource.ValuesResource.UpdateRequest updateSheetCellRequest = sheetService.Spreadsheets.Values.Update(valueRange, spreadsheetId, rangeApiKey);
            updateSheetCellRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            updateSheetCellRequest.Execute();

            valueData.Clear();
            valueData.Add(new List<object>());
            valueData[0].Add(StaticData.URL_WARCRAFTLOGS_REPORTS + "aLqrxJj2vnQ7zYDA" + "/");
            valueRange = new ValueRange();
            valueRange.Values = valueData;

            updateSheetCellRequest = sheetService.Spreadsheets.Values.Update(valueRange, spreadsheetId, rangeLogId);
            updateSheetCellRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            updateSheetCellRequest.Execute();
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

            string path = BASE_URL + $"v1/reports/guild/{guild.Replace(" ", "%20").ToLower()}/{server.ToLower()}/{RegionToString(region)}?start={fromUnixStamp}&end={toUnixStamp}&api_key={_apiKey}";
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

        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
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
                    raid2 = titleToUse.Contains("BT");

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
    }
}
