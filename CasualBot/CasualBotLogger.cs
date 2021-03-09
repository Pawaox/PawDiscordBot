using PawDiscordBot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualBot
{
    public class CasualBotLogger : IPawDiscordBotLogger
    {
        public string LogFilePath { get; private set; }

        public CasualBotLogger(string filePath, bool resetFile)
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
                if (!File.Exists(LogFilePath))
                {
                    File.Create(LogFilePath).Close();
                    File.AppendAllText(LogFilePath, message);
                }
                else
                    File.AppendAllText(LogFilePath, Environment.NewLine + message);

                Console.WriteLine(message);
            }
        }
    }
}
