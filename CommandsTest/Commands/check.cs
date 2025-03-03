using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
using onnaMUD.Database;
//using onnaMUD.Accounts;
//using onnaMUD.Rooms;
using onnaMUD.BaseClasses;

public class Check : ICommand
{
    public Check()
    {
        Aliases = new[] { "check" };
        AllowedUsers = AccountType.Admin;//for now, this is only an admin command. down the road it may be used for something else, we'll add checks for those when we need to
        AllowedInRT = true;

    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        if (command.Length == 1)
        {
            player.OutputToMain("<br>Available things to check are: OBJECTS");
            //ServerFunctions.SendData(player, "<br>Available things to check are: OBJECTS");
            return 0;
        }
        string outputString = "";

        switch (command[1].ToLower())
        {
            case "objects":
                //checks database for any roomObjects that have been created, but not used yet
                List<RoomObject> allObjects = DB.DBAccess.GetList<RoomObject>(DB.Collections.RoomObject);

                for (int i = 0; i < player.CurrentServer.roomCache.Count; i++)
                {
                    for (int j = 0; j < player.CurrentServer.roomCache[i].Objects.Count; j++)
                    {
                        //go through all the objects in all the rooms and check if any RoomObjects from list are here
                        for (int k = allObjects.Count - 1; k >= 0; k--)
                        {
                            //go through the list backwards so we don't screw up the indexes while looping
                            if (player.CurrentServer.roomCache[i].Objects[j] == allObjects[k])
                            {
                                //if these objects match, remove it from the allObjects list so we know it's been used at least once in a room
                                allObjects.RemoveAt(k);
                            }
                        }
                    }
                }

                if (allObjects.Count > 0)
                {
                    outputString += "<br>Unused RoomObjects found:";
                    //ServerFunctions.SendData(player, "Unused RoomObjects found:");
                    for (int i = 0; i < allObjects.Count; i++)
                    {
                        outputString += $"<br>{allObjects[i].Id}";
                        //ServerFunctions.SendData(player, $"{allObjects[i].Id}");
                    }
                } else
                {
                    outputString += "<br>All RoomObjects are being used.";
                    //ServerFunctions.SendData(player, "All RoomObjects are being used.");
                }

         /*       for (int i = 0; i < allObjects.Count; i++)
                {
                    for (int j = 0; j < player.CurrentServer.roomCache.Count; j++)
                    {
                        //for each object in the database, go through each room and check to see if that object has been used yet
                        for (int k = 0; k < player.CurrentServer.roomCache[j].Objects.Count; k++)
                        {
                            if (player.CurrentServer.roomCache[j].Objects[k].Id == allObjects[i].Id)
                            {
                                //if this matches, then this object has been used

                            }

                        }

                    }


                }*/


                break;



        }

        player.OutputToMain(outputString);
        //ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

