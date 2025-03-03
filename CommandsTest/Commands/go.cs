using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;

public class Go : ICommand
{
    public Go()
    {
        Aliases = new[] { "go" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = false;
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        string outputString = "";

        ServerFunctions.SendData(player, "<br>Yes, testing works.");//  command);
                                                                //server.logFile.WriteLine("we executed test!!!!");


        //ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

