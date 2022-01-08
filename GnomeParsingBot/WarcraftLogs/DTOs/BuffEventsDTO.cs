using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs.DTOs
{
    public class BuffEventsDTO
    {
        public Event[] events { get; set; }
        public int count { get; set; }

        public class Event
        {
            public int timestamp { get; set; }
            public string type { get; set; }
            public int sourceID { get; set; }
            public bool sourceIsFriendly { get; set; }
            public int targetID { get; set; }
            public bool targetIsFriendly { get; set; }
            public Ability ability { get; set; }
            public int fight { get; set; }
        }

        public class Ability
        {
            public string name { get; set; }
            public int guid { get; set; }
            public int type { get; set; }
            public string abilityIcon { get; set; }
        }
    }
}
