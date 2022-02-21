using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs.DTOs
{
    public class DamageTakenEventsDTO
    {
        public Event[] events { get; set; }
        public int count { get; set; }
        public int nextPageTimestamp { get; set; }
        public Auraability[] auraAbilities { get; set; }

        public class Event
        {
            public int timestamp { get; set; }
            public string type { get; set; }
            public int sourceID { get; set; }
            public int sourceInstance { get; set; }
            public bool sourceIsFriendly { get; set; }
            public int targetID { get; set; }
            public bool targetIsFriendly { get; set; }
            public Ability ability { get; set; }
            public int fight { get; set; }
            public string buffs { get; set; }
            public int hitType { get; set; }
            public int amount { get; set; }
            public int mitigated { get; set; }
            public int unmitigatedAmount { get; set; }
            public int absorbed { get; set; }
            public int resourceActor { get; set; }
            public Classresource[] classResources { get; set; }
            public int hitPoints { get; set; }
            public int maxHitPoints { get; set; }
            public int attackPower { get; set; }
            public int spellPower { get; set; }
            public int armor { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int facing { get; set; }
            public int mapID { get; set; }
            public int itemLevel { get; set; }
            public int resisted { get; set; }
            public int overkill { get; set; }
            public int blocked { get; set; }
            public bool tick { get; set; }
            public Source source { get; set; }
        }

        public class Ability
        {
            public string name { get; set; }
            public int guid { get; set; }
            public int type { get; set; }
            public string abilityIcon { get; set; }
        }

        public class Source
        {
            public string name { get; set; }
            public int id { get; set; }
            public int guid { get; set; }
            public string type { get; set; }
            public string icon { get; set; }
        }

        public class Classresource
        {
            public int amount { get; set; }
            public int max { get; set; }
            public int type { get; set; }
        }

        public class Auraability
        {
            public string name { get; set; }
            public int guid { get; set; }
            public int type { get; set; }
            public string abilityIcon { get; set; }
        }
    }
}
