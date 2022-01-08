using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.GoogleAPI
{
    internal class RolePerformanceBreakdownException : Exception
    {
        public RolePerformanceBreakdownException(string msg) : base(msg)
        {
        }
    }
}
