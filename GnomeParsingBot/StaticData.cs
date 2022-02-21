using GnomeParsingBot.WarcraftLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot
{
    internal abstract class StaticData
    {
        public const string URL_WARCRAFTLOGS = "https://classic.warcraftlogs.com:443/";
        public const string URL_WARCRAFTLOGS_BROWSERREPORTS = "https://classic.warcraftlogs.com/reports/";

        public const string PATH_KEY_BASE = "C:/apiKey_wlogs_base.txt";
        public const string PATH_KEY_AWARDS = "C:/apiKey_wlogs_awards.txt";

        public const string PATH_FILEDATASTORE_CREDENTIALS = "gnomeParseGoogleCreds7";
        public const string PATH_GOOGLECRED_SERVICE = "C:/gnomeparsing_google_credentials_serviceUser.json";
        public const string PATH_GOOGLECRED_OAUTH = "C:/gnomeparsing_google_credentials_oauth.json";

        public static string WarcraftLogsGuildName { get; set; }
        public static string WarcraftLogsServer { get; set; }
        public static WarcraftLogsClient.Region WarcraftLogsRegion { get; set; }

        public static bool Initialized { get; private set; }

        public static Dictionary<string, ulong> CharactersToDiscordID { get; private set; }

        public static Dictionary<string, Tuple<string, string>> CharactersToRoles { get; private set; }
        static StaticData()
        {
            WarcraftLogsGuildName = "";
            WarcraftLogsServer = "";
        }

        public static void Initialize(string guildName, string server, WarcraftLogsClient.Region region)
        {
            WarcraftLogsGuildName = guildName;
            WarcraftLogsServer = server;
            WarcraftLogsRegion = region;

            CharactersToDiscordID = new Dictionary<string, ulong>();
            AddCharacter(248853951217139713, "Dervz", "Drdervz", "Dervzxiao");
            AddCharacter(312127598719139840, "Cleanst");
            AddCharacter(120938656788643841, "Lazyturtlez", "Lazytree");
            AddCharacter(625706404711235594, "Theseis", "Hieria", "Procsh");
            AddCharacter(377542823974797325, "Keplerr");
            AddCharacter(236483993539837952, "Degree", "Celvin");
            AddCharacter(273806201093881866, "Klazu", "Klasu", "Bonskí");
            AddCharacter(382318323070664724, "Chriiz", "Chriizzy");
            AddCharacter(337591093082193921, "Libidx", "Libixd");
            AddCharacter(130048712297676801, "Phazin", "Phazinamage");
            AddCharacter(190537089048117248, "Vegitomato", "Custardcake", "Kiwibikini", "Letok");
            AddCharacter(205642483206455296, "Morphix", "Titån");
            AddCharacter(408318643466797068, "Whoiz", "Whoizzy");
            AddCharacter(214102947821256705, "Feightw", "Feighto", "Feights", "Feigh", "Feighd", "Feightm");
            AddCharacter(314447596984860673, "Pooi", "Pooip");
            AddCharacter(136534046502158336, "Xoawapdrol", "Pawox", "Falinrush", "Hardsnake");
            AddCharacter(195866189526859776, "Theuntamed", "Talonpriest");
            AddCharacter(329275162874740737, "Consita", "Consitaz", "Consitaq", "Consitaqq", "Consitaqt");
            AddCharacter(276377737126608896, "Casyl", "Casyllock");
            AddCharacter(144019457679818752, "Mitneun", "Mitneunzwei");
            AddCharacter(377262598388711425, "Gamblerp", "Gambleri");
            AddCharacter(247855886415036416, "Uncarryable", "Likemybubble");
            AddCharacter(239850845070557184, "Bangh", "Banghtwo");
            AddCharacter(190120195694657537, "Flowa", "Ranheim", "Haupin", "Goblinlover");
            AddCharacter(448219478866657295, "Bigbrainx", "Verybigman");
            AddCharacter(194891204972773377, "Hootski");
            AddCharacter(288787319392043010, "Maironx");
            AddCharacter(616672337651171329, "Tripingbalz", "Toomuchacid", "Supershaman");
            AddCharacter(219046026068230144, "Vappú", "Vappuq");
            AddCharacter(291665036143886337, "Mistubere", "Húnkatten");


            AddCharacter(402969947304624153, "Woodi");

            CharactersToRoles = new Dictionary<string, Tuple<string, string>>();
            //Druids
            CharactersToRoles.Add("Mitneun", new Tuple<string, string>("Tank", ""));
            CharactersToRoles.Add("Talonpriest", new Tuple<string, string>("Tank", ""));
            CharactersToRoles.Add("Bigbrainx", new Tuple<string, string>("Tank", ""));
            CharactersToRoles.Add("Uncarryable", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Mistubere", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Consitaq", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Jerkie", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Tripingbalz", new Tuple<string, string>("Caster", ""));

            //Hunters
            CharactersToRoles.Add("Falinrush", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Libidx", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Theseis", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Maironx", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Theuntamed", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Ellhunt", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Woodi", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Brokenfang", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Domartarden", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Balg", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Pistolnicke", new Tuple<string, string>("Physical", ""));

            //Mages
            CharactersToRoles.Add("Dervz", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Vappuq", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Dervzxiao", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Bloodykael", new Tuple<string, string>("Caster", ""));

            //Paladins
            CharactersToRoles.Add("Casyl", new Tuple<string, string>("Tank", ""));
            CharactersToRoles.Add("Flowa", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Klazu", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Custardcake", new Tuple<string, string>("Tank", ""));
            CharactersToRoles.Add("Likemybubble", new Tuple<string, string>("Healer", ""));
            
            //ShadowPriest
            CharactersToRoles.Add("Cleanst", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Feighto", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Pooip", new Tuple<string, string>("Caster", ""));

            //Healer Priests
            CharactersToRoles.Add("Gamblerp", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Hieria", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Mitneunzwei", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Schmiddy", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Elroky", new Tuple<string, string>("Healer", ""));
            
            //Rogues
            CharactersToRoles.Add("Degree", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Toomuchacid", new Tuple<string, string>("Physical", ""));

            //Shaman
            CharactersToRoles.Add("Consitaz", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Phazin", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Whoiz", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Milaax", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Procsh", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Vegitomato", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Hootski", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Ixxii", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Verybigman", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Bangh", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Vappú", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Edz", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Hardsnake", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Goblinlover", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Shockntroll", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Catshaman", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Malwina", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Gambleri", new Tuple<string, string>("Caster", ""));
            
            //Warlocks
            CharactersToRoles.Add("Drdervz", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Xoawapdrol", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Feightw", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Lazyturtlez", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Morphix", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Pooi", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Consitay", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Casyllock", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Anderxz", new Tuple<string, string>("Caster", ""));

            //Browns
            CharactersToRoles.Add("Chriiz", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Keplerr", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Libixd", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Whoizzy", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Ranheim", new Tuple<string, string>("Physical", ""));


            #region CharactersToRoles  PUGs
            CharactersToRoles.Add("Aimedshotmfk", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Travesti", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Gogie", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Wingchong", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Flusher", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Winron", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Elementalord", new Tuple<string, string>("Physical", ""));
            CharactersToRoles.Add("Lövlasse", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Bonskí", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Donalda", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Wiretap", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Akraat", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Mooike", new Tuple<string, string>("Tank", ""));
            CharactersToRoles.Add("Orcanizm", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Codeincookie", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Czzeus", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Rankadin", new Tuple<string, string>("Tank", ""));
            CharactersToRoles.Add("Onal", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Eloz", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Parempilmari", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Chainee", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Admiirall", new Tuple<string, string>("Healer", ""));
            CharactersToRoles.Add("Zloow", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Dárkwíng", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Speeded", new Tuple<string, string>("Caster", ""));
            CharactersToRoles.Add("Ikélos", new Tuple<string, string>("Physical", ""));
            #endregion

            RoleAward.RoleIDs = new Dictionary<RoleAward.AwardType, ulong>();
            RoleAward.RoleIDs.Add(RoleAward.AwardType.DEATHS, 927580031989993512);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.BOSSDAMAGE, 927570098535923722);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.TRASHDAMAGE, 927570499414933524);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.CONSUMES, 927557545760002048);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.ENGINEERING, 928475120002007111);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.DRUMSCORE, 927568418301943848);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.CHICKEN, 927569145720111114);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.AVOIDABLEDAMAGETAKEN_MOST, 927569610327355472);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.AVOIDABLEDAMAGETAKEN_LEAST, 928472160748568586);

            RoleAward.RoleIDs.Add(RoleAward.AwardType.HEALING, 927576056091783218);
            RoleAward.RoleIDs.Add(RoleAward.AwardType.HEALER_PACIFISM, 927576056091783218);

            Initialized = true;
        }
        private static void AddCharacter(ulong id, params string[] names)
        {
            foreach (string name in names)
                CharactersToDiscordID.Add(name.ToUpper(), id);
        }

        public static ulong? GetDiscordIDFromCharacterName(string name)
        {
            ulong? id = null;

            name = name?.ToUpper()?.Trim() ?? "";

            if (!string.IsNullOrEmpty(name) && CharactersToDiscordID.ContainsKey(name))
                id = CharactersToDiscordID[name];

            return id;
        }
    }
}
