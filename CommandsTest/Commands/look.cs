using System;
using System.Threading.Tasks;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;

public class Look : ICommand
{
    public Look()
    {
        Aliases = new[] { "l", "look" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = true;
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        string outputString = "";
        //string exits = 

        //default look = show room
        outputString += $"<br>[{room.Name}]";
        //ServerFunctions.SendData(player, "111", $"<br>[{room.Name}]");
        //ServerFunctions.SendData(player, $"{room.Description}");
        //ServerFunctions.SendData(player, $"Obvious paths: {GetExitString(room)}");
        outputString += $"<br>{room.Description}<br>Obvious paths: {GetExitString(room)}";


        //start a RT for testing
        //player.Roundtime = new Timer(player.EndRoundtime, player, 4000, 0);//this SHOULD send the > after 4 seconds once


        //ServerFunctions.SendData(player, $"Name: {player.CharName}, Race: {player.Race}");

        //ServerFunctions.SendData(player, "Yes, testing works.");//  command);
        //server.logFile.WriteLine("we executed test!!!!");

        ServerFunctions.SendData(player, outputString);

        return 0;
    }

    public string GetExitString(Room room)
    {
        string exits = "none";




        return exits;
    }
}

