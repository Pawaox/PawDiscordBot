using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs.DTOs
{
    public class DeathsDTO
    {
        public Entry[] entries { get; set; }

        public class Entry
        {
            public string name { get; set; }
            public int id { get; set; }
            public int guid { get; set; }
            public string type { get; set; }
            public string icon { get; set; }
            public int timestamp { get; set; }
            public Damage damage { get; set; }
            public Healing healing { get; set; }
            public int fight { get; set; }
            public int deathWindow { get; set; }
            public int overkill { get; set; }
            public Event[] events { get; set; }
            public Killingblow killingBlow { get; set; }
        }

        public class Damage
        {
            public int total { get; set; }
            public int activeTime { get; set; }
            public int activeTimeReduced { get; set; }
            public int overheal { get; set; }
            public Ability[] abilities { get; set; }
            public object[] damageAbilities { get; set; }
            public Source[] sources { get; set; }
            public int blocked { get; set; }
        }

        public class Ability
        {
            public string name { get; set; }
            public int total { get; set; }
            public int type { get; set; }
            public string petName { get; set; }
        }

        public class Source
        {
            public string name { get; set; }
            public int total { get; set; }
            public string type { get; set; }
        }

        public class Healing
        {
            public int total { get; set; }
            public int activeTime { get; set; }
            public int activeTimeReduced { get; set; }
            public Ability1[] abilities { get; set; }
            public Damageability[] damageAbilities { get; set; }
            public Source1[] sources { get; set; }
        }

        public class Ability1
        {
            public string name { get; set; }
            public int total { get; set; }
            public int type { get; set; }
        }

        public class Damageability
        {
            public string name { get; set; }
            public int total { get; set; }
            public int type { get; set; }
        }

        public class Source1
        {
            public string name { get; set; }
            public int total { get; set; }
            public string type { get; set; }
        }

        public class Killingblow
        {
            public string name { get; set; }
            public int guid { get; set; }
            public int type { get; set; }
            public string abilityIcon { get; set; }
        }

        public class Event
        {
            public int timestamp { get; set; }
            public string type { get; set; }
            public int sourceID { get; set; }
            public int sourceInstance { get; set; }
            public bool sourceIsFriendly { get; set; }
            public int targetID { get; set; }
            public bool targetIsFriendly { get; set; }
            public Ability2 ability { get; set; }
            public int fight { get; set; }
            public int hitType { get; set; }
            public int amount { get; set; }
            public int mitigated { get; set; }
            public int unmitigatedAmount { get; set; }
            public int overkill { get; set; }
            public int sourceMarker { get; set; }
            public int resisted { get; set; }
            public int blocked { get; set; }
            public Source2 source { get; set; }
            public int targetMarker { get; set; }
        }

        public class Ability2
        {
            public string name { get; set; }
            public int guid { get; set; }
            public int type { get; set; }
            public string abilityIcon { get; set; }
        }

        public class Source2
        {
            public string name { get; set; }
            public int id { get; set; }
            public int guid { get; set; }
            public string type { get; set; }
            public string icon { get; set; }
        }
    }
}
