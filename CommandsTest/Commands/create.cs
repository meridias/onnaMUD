using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
//using onnaMUD.Accounts;
//using onnaMUD.Rooms;
using onnaMUD.Database;

public class Create : ICommand
{
    public Create()
    {
        Aliases = new[] { "create" };
        AllowedUsers = AccountType.Admin;
        AllowedInRT = true;
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)//return RT as int, 0 for no RT
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        string outputString = "";

        if (command.Length == 1)
        {
            ServerFunctions.SendData(player, "<br>Available create options are: ROOM, OBJECT, AREA, CHARACTER");
            return 0;
        }

        string[] options = { "room", "object", "area", "character" };
        //wonder if this will work
        options = options.OrderBy(x => x.ToLower()).ToArray();//works

        //just checking
      //  for (int i = 0; i < options.Length; i++)
        //{
          //  ServerFunctions.SendData(player, options[i]);

        //}

        switch (command[1].ToLower())
        {
            case "room":
                break;
            case "object":
                //room object
                RoomObject newRoomObject = new RoomObject();
                newRoomObject.Id = DB.DBAccess.GetNewGuid<RoomObject>(DB.Collections.RoomObject);
                DB.DBAccess.Save(newRoomObject, DB.Collections.RoomObject);
                ServerFunctions.SendData(player, $"<br>New room object created with ID:{newRoomObject.Id}");
                break;
            case "area":
                break;
            case "character":
                //npcs and mobs, NOT player characters
                break;
            default:
                ServerFunctions.SendData(player, "<br>Available create options are: ROOM, OBJECT, AREA, CHARACTER");
                break;


        }

        //ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

