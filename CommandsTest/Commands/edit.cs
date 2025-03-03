using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
using onnaMUD.Temps;
using onnaMUD.Database;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
using Newtonsoft.Json;
using onnaMUD.Settings;
//using static onnaMUD.Utilities.ServerConsole;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

public class Edit : ICommand
{
    public Edit()
    {
        Aliases = new[] { "edit" };
        AllowedUsers = AccountType.Admin;
        AllowedInRT = true;
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }

    public int Execute(ServerMain server, Player player, Room room, string[] command)
    {
        //if a command has a RT, make sure you add the Roundtime string to the output
        string outputString = "";

        //string[] split = command.Split(' ');
        string thingToEdit = "";
        string editOption = "";
        //string choice = "";
        //ServerFunctions.SendData(player, "testing");
        //if (command.Length == 1)
       // {
            //no options, just 'edit' so show options
       //     ServerFunctions.SendData(player, "Available options for EDIT are: ROOM, OBJECT, REGION, CHARACTER");
       //     return;
       // }//moved this down to ""/default in switch

        if (command.Length > 1)
        {
            //if we have a potential option
            thingToEdit = command[1];
        }
        if (command.Length > 2)
        {
            //for now, assuming 'edit room room.guid' for editing specific room or 'edit room update room.guid'
            editOption = command[2];
        }

        string[] options = { "room", "object", "area", "character" };//for typed out things to edit, don't include 'update' since that will be from a script/button press and will always be 'update'
        //commandList = commandList.OrderBy(x => x.commandAlias).ToList();
        //options = options.OrderBy(x => x.)

        string[] optionsArray = options.OrderBy(x => x.ToLower()).ToArray();

        if (thingToEdit != "update" && thingToEdit.Length > 0)
        {
            //thingToEdit is not 'update' and is not ""
            for (int i = 0; i < optionsArray.Length; i++)
            {
                if (optionsArray[i].StartsWith(thingToEdit.ToLower()))
                {
                    thingToEdit = optionsArray[i];
                    //ServerFunctions.SendData(player, $"matched {optionsArray[i]}");
                    break;
                }
                //if we get to this point, then no match. change thingToEdit to ""?
                //thingToEdit = "";
            }
        } else
        {
            if (thingToEdit == "update" && command.Length > 2)
            {
                //thingToEdit is update, and we have something being updated
                for (int i = 0; i < options.Length; i++)
                {
                    if (optionsArray[i].StartsWith(editOption.ToLower()))
                    {
                        editOption = optionsArray[i];
                        break;
                    }
                    //if we get to this point, then no match. change thingToEdit to ""?
                    editOption = "";
                }
            } else
            {
                editOption = "";
            }


        }

        //ServerFunctions.SendData(player, $"blah:{thingToEdit}");
        //ServerFunctions.SendData(player, $"blah:{editOption}");

        switch (thingToEdit.ToLower())
        {
            case "room"://edit room
                switch (editOption.ToLower())//edit room blah
                {
          /*          case "update"://for receiving a json update for a room.guid (which would be command[3])
                        //check if room.guid is in database already, and get a copy of that
                        //then check update against db copy for changes for logs?
                        //then update db
                        Room roomUpdate = JsonConvert.DeserializeObject<Room>(command[3]);
                        DB.DBAccess.Save<Room>(roomUpdate, DB.Collections.Room);
                        break;*/
                    case ""://no editOption so just 'edit room', edits current room we're in
                        Room baseRoom = ServerFunctions.GetRoom(player, true);
                        string roomJson = JsonConvert.SerializeObject(baseRoom);//, Formatting.Indented);
//                    ServerFunctions.SendData(player, "121", $"room::{ServerFunctions.GetBaseClassString("room")}");
                        ServerFunctions.SendData(player, "052", $"edit::room::{roomJson}");
                        ServerFunctions.SendData(player, "<br>Let's do some editing...");
                        //ServerFunctions.SendData(player, "120", $"room::{roomJson}");
                        break;
                    default://'edit room room.guid'?
                        //check command[2] for valid room.guid id from database
                        ServerFunctions.SendData(player, "roomdefault?");
                        break;
                }

                //if no other options past this, assuming 'edit this room' so get what room we're in and send that info to the client
  /*              if (command.Length == 2)
                {//"edit room"
                    

                    Room baseRoom = ServerFunctions.GetRoom(player, true);
                    string roomJson = JsonConvert.SerializeObject(baseRoom);//, Formatting.Indented);
//                    ServerFunctions.SendData(player, "121", $"room::{ServerFunctions.GetBaseClassString("room")}");
                    ServerFunctions.SendData(player, "060", "edit::open");
                    ServerFunctions.SendData(player, "120", $"room::{roomJson}");
                }*/

                break;

            case "character":
                //ServerFunctions.SendData(player, "edit character?");
                switch (editOption.ToLower())//edit room blah
                {
                    /*           case "update"://for receiving a json update for a room.guid (which would be command[3])
                                   //check if room.guid is in database already, and get a copy of that
                                   //then check update against db copy for changes for logs?
                                   //then update db
                                   Character charUpdate = JsonConvert.DeserializeObject<Character>(command[3]);
                                   DB.DBAccess.Save<Character>(charUpdate, DB.Collections.Character);
                                   break;*/
                    case ""://no editOption so just 'edit character', edits our current player character
                            //Character baseRoom = ServerFunctions.GetRoom(player, true);
                        Character charToEdit = new Character();
                        //charToEdit = (Character)ServerFunctions.UpdateObject(charToEdit, player);
                        ServerFunctions.UpdateObject(charToEdit, player);

                        //ServerFunctions.SendData(player, $"character: ({charToEdit.CharName})");
                        //ServerConsole.newConsole.WriteLine(player.Name);
                        //Character temp = player.GetType().BaseType.GetProperties(BindingFlags.Public);//apparently this doesn't work. screws up JsonConvert?
                        //ServerConsole.newConsole.WriteLine(temp.Name);
                        //ServerConsole.newConsole.WriteLine(player.GetType().BaseType.GetProperties(BindingFlags.Public));
                        string charJson = JsonConvert.SerializeObject(charToEdit);// player.character);//  charToEdit);
                        //ServerConsole.newConsole.WriteLine($"blah:{charJson}");
                        //ServerFunctions.SendData(player, charJson);
                        ServerFunctions.SendData(player, "052", $"edit::character::{charJson}");
                        ServerFunctions.SendData(player, "<br>Let's do some editing...");
                        //ServerFunctions.SendData(player, "120", $"character::{charJson}");

                        /*        foreach (var sourceF in charT.GetProperties())//  GetFields())//get all fields from character class type
                                {
                                    //ServerConsole.newConsole.WriteLine($"{sourceF.Name}");
                                    var playerF = playerT.GetProperty(sourceF.Name);//  GetField(sourceF.Name, BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                                                                                    //ServerConsole.newConsole.WriteLine($"Name: {playerF}");
                                    if (playerF == null)//if no matching field/property name from character class, skip and go to next
                                        continue;
                                    playerF.SetValue(player, sourceF.GetValue(characterList[option], null), null);//set value of character property to player property
                                }*/

                        //string roomJson = JsonConvert.SerializeObject(baseRoom);//, Formatting.Indented);
                        //                    ServerFunctions.SendData(player, "121", $"room::{ServerFunctions.GetBaseClassString("room")}");
                        //ServerFunctions.SendData(player, "052", "edit::room");
                        //ServerFunctions.SendData(player, "120", $"room::{roomJson}");
                        break;
                    default://'edit room room.guid'?
                        //check command[2] for valid room.guid id from database
                        ServerFunctions.SendData(player, "chardefault?");
                        break;
                }
                break;
            case "object":
                break;
            case "area":
                break;

            case "update":
                switch (editOption)
                {
                    case "area":
                        break;
                    case "object":
                        break;
                    case "room":
                        Room roomUpdate = JsonConvert.DeserializeObject<Room>(command[3]);
                        if (DB.DBAccess.GetById<Room>(roomUpdate.Id, DB.Collections.Room) == null)
                        {
                            ServerFunctions.SendData(player, "<br>Room not found in DB.");
                            return 0;
                        }

                        DB.DBAccess.Save<Room>(roomUpdate, DB.Collections.Room);
                        ServerFunctions.SendData(player, "<br>Room base updated.");
                        //update room in roomCache and send new room info to players in that room...
                        break;

                    case "character":
                        Character charUpdate = JsonConvert.DeserializeObject<Character>(command[3]);
                        //check if character is actually already in the DB?
                        if (DB.DBAccess.GetById<Character>(charUpdate.Id, DB.Collections.Character) == null)
                        {
                            //character ID not found
                            ServerFunctions.SendData(player, "<br>Character not found in DB. Please check character.");
                            return 0;
                        }

                        //do checks for name
                        if (!ServerFunctions.IsCharNameAllowed(player, charUpdate.Name))
                        {
                            //name is not allowed, error messages are already sent from IsCharNameAllowed method so we don't need to add anything here to the player
                            return 0;
                        }


                        DB.DBAccess.Save<Character>(charUpdate, DB.Collections.Character);
                        //ServerFunctions.UpdateCharOnPlayer(player, charUpdate);
                        //checks for if character is currently being played by player?
                        player.character = charUpdate;
                        //ServerFunctions.UpdateObject(player, charUpdate);
                        //player.character = charUpdate;

                        ServerFunctions.SendData(player, "<br>Character updated.");
                        break;

                    default:
                        break;

                }

                break;

 /*           case "self":
                if (command.Length > 2 && command[2].ToLower() == "done")// && player.tempScript.GetType().Name == "CharManager")
                {//if we send EDIT SELF DONE and the tempscript is CharManager, then we're saying we're done editing
                    //so check if new character, get a new ID first, THEN
                    //insert/update character into database
                    goto case "done";
  //                  if (player.Id == Guid.Empty)
  //                  {
  //                      player.Id = DB.DBAccess.GetNewGuid<Character>(DB.Collections.Character);
  //                  }
  //                  DB.DBAccess.Save<Character>(player, DB.Collections.Character);
  //                  ServerFunctions.SendData(player, "Character saved!");
                }
                if (player.tempScript != null && player.tempScript.GetType().Name == "CharManager")
                {
                    ServerFunctions.SendData(player, "You have already enabled editing your character. Please CHOOSE your options and type EDIT SELF DONE when you're finished.");
                }
                else
                {
                    ServerFunctions.SendData(player, "You can now access the Character Manager and change your appearance with CHOOSE. When done, type EDIT SELF DONE");
                    player.tempScript = new CharManager(player, player);
                }
                break;
            case "done":
                //this is only for confirming self character editing
                if (player.tempScript != null && player.tempScript.GetType().Name == "CharManager")
                {
                    if (player.Id == Guid.Empty)
                    {
                        player.Id = DB.DBAccess.GetNewGuid<Character>(DB.Collections.Character);
                        //ServerFunctions.SendData(player, player.Id.ToString());
                    }
                    //Character temp = (Character)player;

                    DB.DBAccess.Save<Character>(player, DB.Collections.Character);
                    ServerFunctions.SendData(player, "Character saved!");

                }
                break;*/
            default:
                ServerFunctions.SendData(player, "<br>Available options for EDIT are: ROOM, OBJECT, REGION, CHARACTER");
                break;

        }

        //ServerFunctions.SendData(player, "Yes, testing works.");//  command);
        //server.logFile.WriteLine("we executed test!!!!");

        //ServerFunctions.SendData(player, outputString);

        return 0;
    }
}

