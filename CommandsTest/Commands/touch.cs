using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
//using onnaMUD.Accounts;
using System.Diagnostics.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using onnaMUD.Rooms;

public class Touch : ICommand
{
    public Touch()
    {
        Aliases = new[] { "touch" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = false; //maybe true? have to come back and check on this
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        string outputString = "";

        //ServerFunctions.SendData(player, "Yes, testing works.");//  command);
        if (command.Length == 1)
        {
            ServerFunctions.SendData(player, "<br>Touch what?");
            return 0;
        }

        var matchedObject = ServerFunctions.MatchObjectInRoom(player, room, command[1]);
        //for now, let's just assume last command is what we're matching
        //ServerFunctions.SendData(player, $"{room.Stuff.Count}");
        /*       for (int i = 0; i < room.Stuff.Count; i++)
               {
                   if (room.Stuff[i].ThingType == ThingType.Character)
                   {
                       if (((Character)room.Stuff[i]).CharName.ToLower().StartsWith(command[1].ToLower()))
                       {
                           ServerFunctions.SendData(player, $"You reach out and touch {((Character)room.Stuff[i]).CharName}.");
                           return;
                       }
                       //ServerFunctions.SendData(player, $"{((Character)room.Stuff[i]).CharName}");
                       //ServerFunctions.SendData(player, $"{room.Stuff[i].CharName}")
                   }

               }*/

        if (matchedObject != null)
        {
            if (matchedObject.ThingType == ThingType.Character)
            {
                Character touchedChar = (Character)matchedObject;
                //if (touchedChar.Player == null)

                if (touchedChar.Player == player)
                {
                    //we just touched ourselves, yes I know how that sounds
                    string playerMessage = "<br>You poke yourself.";
                    string roomMessage = $"<br>{touchedChar.Name} pokes themselves.";
                    OutputFunctions.SendToRoom(room, player, playerMessage, roomMessage);
                    //ServerFunctions.SendData(player, "<br>You poke yourself.");
                } else
                {
                    //if you touched another player
                    string playerMessage = $"<br>You reach out and touch {matchedObject.Name}.";
                    string targetMessage = $"<br>You were poked by {player.character.Name}.";
                    string roomMessage = $"{player.character.Name} reaches out and pokes {touchedChar.Name}.";
                    OutputFunctions.SendToRoom(room, player, playerMessage, touchedChar.Player, targetMessage, roomMessage);
                    //ServerFunctions.SendData((Player)matchedObject, $"<br>You were poked by {player.Name}");
                    //ServerFunctions.SendData(player, $"<br>You reach out and touch {matchedObject.Name}.");
                }
            }
        }
        else
        {
            //ServerFunctions.SendData(player, "<br>You could not find that.");
            player.SendData("<br>You could not find that.");
        }

        //ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

