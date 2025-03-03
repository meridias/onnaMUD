using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
//using onnaMUD.Rooms;
using onnaMUD.BaseClasses;

public class Blank : ICommand
{
    public Blank()
    {
        Aliases = new[] { "blah" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = true;

    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {

        string outputString = "";
        ServerFunctions.SendData(player, "<br>Yes, testing works.");//  command);
        //server.logFile.WriteLine("we executed test!!!!");


        ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

