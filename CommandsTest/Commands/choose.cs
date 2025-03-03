using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
using onnaMUD.BaseClasses;

public class Choose : ICommand
{
    public Choose()
    {
        Aliases = new[] { "choose" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = false;

    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    private string canChoose = "none";

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        string outputString = "";

        if (room.RoomType == RoomType.CharCreate)
        {
            canChoose = "charGen";
        }



 //       if (canChoose == "none")
 //       {
 //           outputString += "<br>What are you trying to choose?";
            //ServerFunctions.SendData(player, "What are you trying to choose?");
 //           return;
 //       }

        switch (canChoose)
        {
            case "charGen":
                outputString += "<br>Character gen....";
                break;
            case "none":
                outputString += "<br>What are you trying to choose?";
                break;


        }


        //if player.Id (not player.AccountId which is id for the account) is Guid.Empty, means that this is a character that hasn't been created yet and so we're in character creation?
        //ServerFunctions.SendData(player, "Yes, testing works.");//  command);

        ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

