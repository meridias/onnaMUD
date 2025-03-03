using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
using onnaMUD.Database;
using System.Runtime.InteropServices;
//using onnaMUD.Accounts;
//using onnaMUD.Rooms;

public class Move : ICommand
{
    public Move()
    {
        Aliases = new[] { "n","e","s","w","ne","se","sw","nw","u","d","up","down","north","east","south","west","southeast","southwest","northeast","northwest","move" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = false;
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        string outputString = "";

        bool isGuid = false;
        Guid toRoomGuid;
        Room toRoom = new Room();

        if (command.Length > 1)
        {
            //if there is 'move n', 'move guid' or 'move blah' here
            isGuid = Guid.TryParse(command[1], out toRoomGuid);
            //ServerFunctions.SendData(player, "have we moved?");
            if (isGuid)
            {
                //if we've being moved to a specific room by guid
                //this needs to be the room that's in the cache, not from the database
                //toRoom = DB.DBAccess.GetById<Room>(toRoomGuid, DB.Collections.Room);
                toRoom = ServerFunctions.GetRoom(player, toRoomGuid);
            }
            else
            {
                //if we're moving by way of the normal directionals, 'move n', 'move up', etc


            }

        } else
        {
            //directionals by themselves? 'n', 'up', etc

        }


        if (toRoom.Region != Guid.Empty)//if the room has an associated region to it
        {
            Region charReg = DB.DBAccess.GetById<Region>(toRoom.Region, DB.Collections.Region);
            if (charReg != null && charReg.IsInstanced)//if we found a region for this room and it is an instanced region
            {//do stuff
                ServerFunctions.SendData(player, "<br>this is a region instance", false);
                //                                break;
            }
            else
            {
                //if we didn't find a region for this room, or it is not an instanced region
                ServerFunctions.SendData(player, "<br>not instanced", false);
            }
        }
        else
        {
            ServerFunctions.SendData(player, "<br>no region", false);
        }


        //     ServerFunctions.SendData(player, "Yes, testing works.");//  command);
        //server.logFile.WriteLine("we executed test!!!!");
        player.character.CurrentRoom = toRoom.Id;
        toRoom.StuffInRoom.Add(player.character);

        //ServerFunctions.SendData(player, outputString);
        //after being moved, do a 'look'
        ServerFunctions.AddCommandToQueue(player, "look");
        return 0;

    }
}

