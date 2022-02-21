using PawDiscordBot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GnomeParsingBot
{
    public class GnomeParsingBotLogger : IPawDiscordBotLogger
    {
        public string LogFilePath { get; private set; }

        public GnomeParsingBotLogger(string filePath, bool resetFile)
        {
            this.LogFilePath = filePath;

            if (resetFile)
            {
                try
                {
                    if (File.Exists(LogFilePath))
                        File.Delete(LogFilePath);
                }
                catch (Exception exc)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                }
            }
        }

        public void Log(string message)
        {
            message = "[" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "] " + message;

            if (!string.IsNullOrEmpty(LogFilePath))
            {
                lock (LogFilePath)
                {
                    if (!File.Exists(LogFilePath))
                    {
                        File.Create(LogFilePath).Close();
                        File.AppendAllText(LogFilePath, message);
                    }
                    else
                        File.AppendAllText(LogFilePath, Environment.NewLine + message);
                }

                Console.WriteLine(message);
            }
        }
    }
}
