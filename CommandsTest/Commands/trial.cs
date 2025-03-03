using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
//using onnaMUD.Rooms;
using onnaMUD.BaseClasses;

public class Trial : ICommand
{
    public Trial()
    {
        Aliases = new[] { "trial" };
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

        if (player.AccountType > AccountType.Trial)
        {
            //isNewTrialStarted = server.IsTrialProcessStarted(player);
            ServerFunctions.SendData(player, "<br>You already have a registered account.");
            //outputString += "<br>You already have a registered account.";
            //ServerFunctions.SendData(player, "You already have a registered account.");
            return 0;
        }

        if (player.AccountID != Guid.Empty)
        {
            //if this player already has an accountID set, then they've already got a trial account started
            //show active time left on this account
            ServerFunctions.SendData(player, "<br>You have blah playable time left on this account.");
            //outputString += "<br>You have blah playable time left on this account.";
            //ServerFunctions.SendData(player, "You have blah playable time left on this account.");
            return 0;
        }


        //ServerFunctions.SendData(player, "Yes, testing works.");//  command);
        //server.logFile.WriteLine("we executed test!!!!");

        ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

