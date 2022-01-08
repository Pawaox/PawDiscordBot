using Discord.WebSocket;
using GnomeParsingBot;
using GnomeParsingBot.GoogleAPI;
using GnomeParsingBot.WarcraftLogs;

try
{
    StaticData.Initialize("Gnome Lovers", "Gehennas", WarcraftLogsClient.Region.EU);

    string botKeyPath = "C:/discordclient_gnomeParsing.txt";
    string botLogPath = "C:/Log_GnomeParsing.txt";
    bool useWS4NetProvider = true;

    string botKey = "";
    if (File.Exists(botKeyPath))
        botKey = File.ReadAllText(botKeyPath);
    else
        throw new Exception("Couldn't find client key at " + botKeyPath);

    DiscordSocketConfig discSocketConfig = new DiscordSocketConfig();
    discSocketConfig.AlwaysDownloadUsers = true;

    if (useWS4NetProvider)
        discSocketConfig.WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance;

    GnomeParsingBotClient client = new GnomeParsingBotClient(botKey);
    client.Logger = new GnomeParsingBotLogger(botLogPath, true);

    client.Start(discSocketConfig);
    
    #region Console Input
    ConsoleColor oldColor = Console.ForegroundColor;
    bool doRun = true;
    while (doRun)
    {
        string? input = Console.ReadLine();

        switch (input?.ToLower() ?? "")
        {
            case "stop":
                doRun = false;

                oldColor = ChangeForeground(ConsoleColor.Yellow);
                client.Logger.Log("Input '" + input + "' - STOPPING...");
                break;
            case "exit":
                doRun = false;

                oldColor = ChangeForeground(ConsoleColor.Yellow);
                client.Logger.Log("Input '" + input + "' - STOPPING...");
                break;
        }
    }
    #endregion

    ChangeForeground(oldColor);
    Thread.Sleep(4000);
}
catch (Exception exc)
{
    Console.WriteLine(exc.ToString());
    Console.Write("Finished, Press <ENTER> to close");
    Console.ReadLine();
}


#region Console Helper Methods
static ConsoleColor ChangeForeground(ConsoleColor newColor)
{
    ConsoleColor oldColor = Console.ForegroundColor;
    Console.ForegroundColor = newColor;
    return oldColor;
}
static ConsoleColor ChangeBackground(ConsoleColor newColor)
{
    ConsoleColor oldColor = Console.BackgroundColor;
    Console.BackgroundColor = newColor;
    return oldColor;
}
#endregion