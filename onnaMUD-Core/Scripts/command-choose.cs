using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
//using onnaMUD.Accounts;
using onnaMUD.Database;
using onnaMUD.Temps;
//using onnaMUD.Rooms;

public class Choose : ICommand
{
    public Choose()
    {
        Aliases = new[] { "choose" };
        AllowedUsers = AccountType.Trial;
        AllowedInRT = true;
    }
    public string[] Aliases { get; }
    public AccountType AllowedUsers { get; }
    public bool AllowedInRT { get; }
    public string canChoose = "none";

    private int menuOption = -1;

    public void Execute(Player player, Room room, string[] command)
    {
        if (player.CurrentServer.serverType == "login") // == null won't work since, before they pick a game server, they'll be on the login server
        {
            //hasn't picked a server yet
            canChoose = "server-select";
        }
        if (canChoose == "none" && player.AccountID != Guid.Empty && player.Id == Guid.Empty && player.CurrentRoom == Guid.Empty)
        {
            //player has logged in with valid account, ID is not set so character has not been picked/new character, and we're not in a room yet so we're at char select
            canChoose = "char-select";
        }
        if (canChoose == "none" && player.AccountID != Guid.Empty && player.Id == Guid.Empty && ServerFunctions.GetRoom(player).RoomType == RoomType.CharCreate)
        {
            //player has logged in with valid account, ID is not set so character has not been picked/new character, and character is in a char creation room
            //so we're at character creation
            canChoose = "create";
        }

        //at this point, none of the above IF's have matched so nothing available to 'choose'
        if (canChoose == "none")
        {
            ServerFunctions.SendData(player, "What are you trying to choose?");
            return;
        }

        //        if (player.tempScript == null)
        //        {
        //no tempScript
        //            ServerFunctions.SendData(player, "What are you trying to choose?");
        //            return;
        //        }

        switch (canChoose)
        {
            case "server-select"://server select
                //shown available servers from DoLogin in ServerFunctions
                //here is where we picking a server if we need to (more than 1)

      /*          List<int> availableServers = new List<int>();
                int chosenServer = 0;
                //check to see if we even need to show server selection
                for (int i = 0; i < ServerFunctions.servers.Count; i++)
                {
                    //ignore login server
                    if (ServerFunctions.servers[i].serverType == "game")
                    {
                        if (ServerFunctions.servers[i].serverIsRunning && (player.AccountType >= AccountType.Mod || ServerFunctions.servers[i].isDefault))
                        {
                            //if this server is up AND
                            //either player is mod/admin or this server is set as a default server
                            //then add it to the list
                            availableServers.Add(i);
                            break;
                        }
                        //if this server is not default, check for if it's up and go through explicit allowed servers on this player
                        if (ServerFunctions.servers[i].serverIsRunning && !ServerFunctions.servers[i].isDefault)
                        {
                            string temp = DB.DBAccess.GetInfo(player, "allowedServers");
                            string[] allowed = temp.Split(',', StringSplitOptions.RemoveEmptyEntries);

                            for (int j = 0; j < allowed.Length; j++)
                            {
                                if (allowed[j] == ServerFunctions.servers[i].serverName)
                                {
                                    availableServers.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (availableServers.Count == 1)
                {
                    //only 1
                    chosenServer = availableServers[0];
                    ServerFunctions.SendData(player, $"Connecting to {ServerFunctions.servers[chosenServer].serverName} server!");
                    //find player in login connections
                    for (int i = 0; i < ServerFunctions.servers.Count; i++)
                    {
                        if (ServerFunctions.servers[i].serverType == "login")
                        {
                            for (int j = 0; j < ServerFunctions.servers[i].connections.Count; j++)
                            {
                                if (ServerFunctions.servers[i].connections[j].player == player)
                                {
                                    //add this player connection from login to connected game server
                                    player.CurrentServer = ServerFunctions.servers[chosenServer];
                                    ServerFunctions.servers[i].logFile.WriteLine($"{player.IP} transfered from login server to {player.CurrentServer.serverName}");
                                    player.CurrentServer.logFile.WriteLine($"{player.IP} transfered from login server to {player.CurrentServer.serverName}");
                                    player.CurrentServer.connections.Add(ServerFunctions.servers[i].connections[j]);
                                    ServerFunctions.servers[i].connections.RemoveAt(j);
                                    //whichChoice = "character";
                                }
                            }
                        }
                    }
                }

                if (availableServers.Count == 0)
                {//no servers in the list
                    ServerFunctions.SendData(player, "There are currently no game servers available. Please try again later.");
                    return;
                }
                //ShowChoices(player);
                //this is for when more than 1 allowed server
                ServerFunctions.SendData(player, "Available servers:<br>");
                for (int i = 0; i < availableServers.Count; i++)
                {
                    ServerFunctions.SendData(player, $"   {i + 1}) <link=\"choose{i + 1}\">{ServerFunctions.servers[availableServers[i]].serverName}</link>");
                }*/
                break;
            case "char-select"://character select
                               //int menuOption = -1;
                               //since textmeshpro link ids can (as far as I can tell) use spaces, we'll use the link ids as 'commands' sending back for basic stuff
                               //'choose' and the like.
                               //this is to get the choice # from either typing 'choose 1' or clicking a link 'choose 1'

                List<Character> characterList = new List<Character>();
                //check for character login/creation
                characterList = DB.DBAccess.GetCharacters(player, player.CurrentServer.serverName);//get the list of characters for this player on this server
                //list is sorted in the GetCharacters function

                //PICK FROM LIST-------------------------------------should be first, so if they haven't picked or picked wrong, show list will pop up
                if (command.Length > 1)
                {//if there is 'choose #'
                    if (!Int32.TryParse(command[1], out menuOption))
                    {
                        ServerFunctions.SendData(player, "Invalid selection! Please enter the number for the option you want.");
                        return;
                    }

                    //hopefully, this works with player.tempScript.    seems to work
                    if (menuOption > 0 && menuOption <= characterList.Count)//  characterList.Count)
                    {
                        //load character and all that stuff
                        //or new character
                        if (characterList[menuOption - 1].Id != Guid.Empty)
                        {
                            //if this is an already created character
                            //we already have the Character class for this character loaded in the list so we just need to copy it to the player
                            var playerT = player.GetType();//get class type for player
                            var charT = characterList[menuOption - 1].GetType();//get type for character class to get field/property
                            foreach (var sourceF in charT.GetProperties())//  GetFields())//get all fields from character class type
                            {
                                //ServerConsole.newConsole.WriteLine($"{sourceF.Name}");
                                var playerF = playerT.GetProperty(sourceF.Name);//  GetField(sourceF.Name, BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                                                                                //ServerConsole.newConsole.WriteLine($"Name: {playerF}");
                                if (playerF == null)//if no matching field/property name from character class, skip and go to next
                                    continue;
                                playerF.SetValue(player, sourceF.GetValue(characterList[menuOption - 1], null), null);//set value of character property to player property
                            }
                            //player.tempScript = null;

                            ServerFunctions.SendData(player, "052", $"character::{player.CharName}");
                            //if somehow, current room is guid.empty, move them to first room? which should be char creation. but if they have
                            //a name, they should be moved to room that charcreation drops them into, once we get to that point of course
                            if (player.CurrentRoom == Guid.Empty)
                            {
                                Room charGen = DB.DBAccess.GetById<Room>(player.CurrentServer.firstRoom, DB.Collections.Room);//  ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                                                                                                                              //Region charReg;
                                if (charGen.Region != Guid.Empty)//if the room has an associated region to it
                                {
                                    Region charReg = DB.DBAccess.GetById<Region>(charGen.Region, DB.Collections.Region);
                                    if (charReg != null && charReg.IsInstanced)//if we found a region for this room and it is an instanced region
                                    {//do stuff
                                        ServerFunctions.SendData(player, "this is a region instance");
                                        //                                break;
                                    }
                                    else
                                    {
                                        //if we didn't find a region for this room, or it is not an instanced region
                                        ServerFunctions.SendData(player, "not instanced");
                                    }
                                }
                                else
                                {
                                    ServerFunctions.SendData(player, "no region");
                                }
                                //ServerFunctions.SendData(player, "no region,or not instanced");
                                player.CurrentRoom = player.CurrentServer.firstRoom;
                            }

                            //ServerFunctions.SendData(player, $"Selected option #{menuOption}");
                            //var character = DB.DBAccess.GetById<Character>(characterList[menuOption - 1].Id, DB.Collections.Character);
                            //player = (Player)character;//  JsonConvert.DeserializeObject<Character>(JsonConvert.SerializeObject(data));
                            //ServerFunctions.SendData(player, $"{(Races)character.Race}");
                            ServerFunctions.AddCommandToQueue(player, "look");
                        }
                        else
                        {
                            //new character
                            //get the first room from the server info
                            //player.tempScript = null;//this copy of CharSelect should still run until end of script? then be GC'd later
                            Room charGen = DB.DBAccess.GetById<Room>(player.CurrentServer.firstRoom, DB.Collections.Room);//  ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                                                                                                                          //Region charReg;
                            if (charGen.Region != Guid.Empty)//if the room has an associated region to it
                            {
                                Region charReg = DB.DBAccess.GetById<Region>(charGen.Region, DB.Collections.Region);
                                if (charReg != null && charReg.IsInstanced)//if we found a region for this room and it is an instanced region
                                {//do stuff
                                    ServerFunctions.SendData(player, "this is a region instance");

                                    //                                break;
                                }
                                else
                                {
                                    //if we didn't find a region for this room, or it is not an instanced region
                                    ServerFunctions.SendData(player, "not instanced");
                                }
                            }
                            else
                            {
                                ServerFunctions.SendData(player, "no region");
                            }
                            //ServerFunctions.SendData(player, "no region,or not instanced");
                            player.CurrentRoom = player.CurrentServer.firstRoom;// ServerFunctions.servers[chosenServer].firstRoom;
                                                                                //player.tempScript = null;
                            player.tempScript = new CharManager(player, player);
                            ServerFunctions.AddCommandToQueue(player, "look");
                        }
                        return;
                    }
                    else
                    {
                        ServerFunctions.SendData(player, "Invalid selection! Please enter the number for the option you want.<br>");
                    }
                }

                //SHOW LIST-------------------------------
                ServerFunctions.ShowCharSelection(player);

                break;

            case "create"://new character creation stuff



                break;


            default:
                //if somehow we got here by not matching any of the above and got past the 'none' check earlier
                ServerFunctions.SendData(player, "What are you trying to choose?");
                break;
        }


        //this is for server/character selection
 /*       if (player.AccountID != Guid.Empty && player.Id == Guid.Empty)
        {
            //accountId is set, character Id is not, so we're at the server/character select

            if (player.CurrentServer.serverType == "login")
            {
                //on login server, so we're at server selection?

            }
            else
            {
                //on game server, so we're at character select
                int menuOption = -1;
                //since textmeshpro link ids can (as far as I can tell) use spaces, we'll use the link ids as 'commands' sending back for basic stuff
                //'choose' and the like.
                //this is to get the choice # from either typing 'choose 1' or clicking a link 'choose 1'

                if (command.Length > 1)
                {
                    if (!Int32.TryParse(command[1], out menuOption))
                    {
                        ServerFunctions.SendData(player, "Invalid selection! Please enter the number for the option you want.");
                        return;
                    }
                }

                //                switch (whichChoice)
                //                {
                //                    case "server":
                //                        break;
                //                    case "character":

                //hopefully, this works with player.tempScript.    seems to work
                if (menuOption > 0 && menuOption <= player.tempScript.characterList.Count)//  characterList.Count)
                {
                    //load character and all that stuff
                    //or new character
                    if (player.tempScript.characterList[menuOption - 1].Id != Guid.Empty)
                    {
                        //if this is an already created character
                        //we already have the Character class for this character loaded in the list so we just need to copy it to the player
                        var playerT = player.GetType();//get class type for player
                        var charT = player.tempScript.characterList[menuOption - 1].GetType();//get type for character class to get field/property
                        foreach (var sourceF in charT.GetProperties())//  GetFields())//get all fields from character class type
                        {
                            //ServerConsole.newConsole.WriteLine($"{sourceF.Name}");
                            var playerF = playerT.GetProperty(sourceF.Name);//  GetField(sourceF.Name, BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                                                                            //ServerConsole.newConsole.WriteLine($"Name: {playerF}");
                            if (playerF == null)//if no matching field/property name from character class, skip and go to next
                                continue;
                            playerF.SetValue(player, sourceF.GetValue(player.tempScript.characterList[menuOption - 1], null), null);//set value of character property to player property
                        }
                        player.tempScript = null;

                        ServerFunctions.SendData(player, "052", $"character::{player.CharName}");
                        //ServerFunctions.SendData(player, $"Selected option #{menuOption}");
                        //var character = DB.DBAccess.GetById<Character>(characterList[menuOption - 1].Id, DB.Collections.Character);
                        //player = (Player)character;//  JsonConvert.DeserializeObject<Character>(JsonConvert.SerializeObject(data));
                        //ServerFunctions.SendData(player, $"{(Races)character.Race}");
                        ServerFunctions.AddCommandToQueue(player, "look");
                    }
                    else
                    {
                        //new character
                        //get the first room from the server info
                        player.tempScript = null;//this copy of CharSelect should still run until end of script? then be GC'd later
                        Room charGen = DB.DBAccess.GetById<Room>(player.CurrentServer.firstRoom, DB.Collections.Room);//  ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                                                                                                                      //Region charReg;
                        if (charGen.Region != Guid.Empty)//if the room has an associated region to it
                        {
                            Region charReg = DB.DBAccess.GetById<Region>(charGen.Region, DB.Collections.Region);
                            if (charReg != null && charReg.IsInstanced)//if we found a region for this room and it is an instanced region
                            {//do stuff
                                ServerFunctions.SendData(player, "this is a region instance");

//                                break;
                            }
                            else
                            {
                                //if we didn't find a region for this room, or it is not an instanced region
                                ServerFunctions.SendData(player, "not instanced");
                            }
                        }
                        else
                        {
                            ServerFunctions.SendData(player, "no region");
                        }
                        //ServerFunctions.SendData(player, "no region,or not instanced");
                        player.CurrentRoom = player.CurrentServer.firstRoom;// ServerFunctions.servers[chosenServer].firstRoom;
                        //player.tempScript = null;
                        player.tempScript = new CharManager(player, player);
                        ServerFunctions.AddCommandToQueue(player, "look");
                    }
                }
                else
                {
                    ServerFunctions.SendData(player, "Invalid selection! Please enter the number for the option you want.");
                }
                //                       break;
                //               }
            }

         

        }*/

        //ServerFunctions.SendData(player, "Yes, testing works.");//  command);
    }
}

