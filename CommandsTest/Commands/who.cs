using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
using onnaMUD.Settings;
//using onnaMUD.Accounts;
//using onnaMUD.Rooms;

public class Who : ICommand
{
    public Who()
    {
        Aliases = new[] { "who" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = false;//just for now, for testing
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        string outputString = "";
        if (player.AccountType >= AccountType.Mod)
        {
            //if mod or admin, show everybody on all servers
            //     for (int i = 0; i < ServerFunctions.servers.Count; i++)
            //     {
            //outputString += $"<br>Number of players on {Config.config.gameName} server: {ServerFunctions.servers[i].connections.Count}";
            outputString += $"<br>Number of players on server: {server.connections.Count}";
                //ServerFunctions.SendData(player, $"Number of players on {ServerFunctions.servers[i].serverName} ({ServerFunctions.servers[i].serverType}) server: {ServerFunctions.servers[i].connections.Count}");
       //     }
            //return;
        } else
        {
            //if not, default to current server
            if (player.CurrentServer != null)
            {
                ServerFunctions.SendData(player, $"<br>Number of players on current server: {player.CurrentServer.connections.Count}");
            } else
            {
                Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? who");
            }
            /*
       //     int index = ServerFunctions.GetCurrentServer(player);
            if (index == -1)
            {
                Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? who");
                return;
            }
            ServerFunctions.SendData(player, $"Number of players on current server: {ServerFunctions.servers[index].connections.Count}");*/
        }

        //player.Roundtime = pla
        //player.Roundtime = 4000;
        //player.RTWatch.Start();

        //ServerFunctions.StartNewTimer(player, 10, "RT", "endRT");
        //ServerFunctions.SendData(player, player.RTWatch.IsRunning.ToString());
        //player.InRT = true;
        //player.Roundtime.Change(4000, 0);
        //player.InRT = true;

        //ServerFunctions.SendData(player, "Yes, testing works.");//  command);
        //server.logFile.WriteLine("we executed test!!!!");

        outputString += $"<br>Roundtime: 10 sec.";
        ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

