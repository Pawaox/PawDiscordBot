using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot.GoogleAPI
{
    internal class AppsScriptException : Exception
    {
        public AppsScriptException(string msg) : base(msg)
        {
        }
    }
}
