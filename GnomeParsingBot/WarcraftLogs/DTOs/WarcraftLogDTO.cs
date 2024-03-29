﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.WarcraftLogs.DTOs
{
    internal class WarcraftLogDTO
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? owner { get; set; }
        public long start { get; set; }
        public long end { get; set; }
        public int zone { get; set; }
    }
}
