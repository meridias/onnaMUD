//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using onnaMUD.Characters;
using onnaMUD.Database;
using onnaMUD.MUDServer;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
using onnaMUD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using onnaMUD.Settings;

namespace onnaMUD.Temps
{
    public class CharSelect
    {
        public List<int> availableServers = new List<int>();
        public int chosenServer = 0;
        //public List<Guid> charGuid = new List<Guid>();
        public List<Character> characterList = new List<Character>();
        public string whichChoice = "server";

        public CharSelect(Player player)
        {
            //check to see if we even need to show server selection
            for (int i = 0; i < ServerFunctions.servers.Count; i++)
            {
                //ignore login server
    //            if (ServerFunctions.servers[i].serverType == "game")
     //           {
                    if (ServerFunctions.servers[i].serverIsRunning && (player.AccountType >= AccountType.Mod))// || ServerFunctions.servers[i].serverInfo.IsDefault))
                    {
                        //if this server is up AND
                        //either player is mod/admin or this server is set as a default server
                        //then add it to the list
                        availableServers.Add(i);
                        break;
                    }
                    //if this server is not default, check for if it's up and go through explicit allowed servers on this player
                    if (ServerFunctions.servers[i].serverIsRunning)// && !ServerFunctions.servers[i].serverInfo.IsDefault)
                    {
                        string temp = DB.DBAccess.GetInfo(player, "allowedServers");
                        //if (string.IsNullOrEmpty(temp))//I think I fixed this in the GetInfo method
                        //{//if the allowedServers is null (because liteDB sets empty strings to null)
                        //  temp = "";//then set the temp to empty string so the split doesn't choke on the null
                        // }
                        string[] allowed = temp.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        for (int j = 0; j < allowed.Length; j++)
                        {
                 //           if (allowed[j] == ServerFunctions.servers[i].serverInfo.Name)
                 //           {
                 //               availableServers.Add(i);
                 //               break;
                 //           }
                        }
                    }
      //          }
            }
            //if there is only 1 server in this list, then just automatically connect to it, if more than 1 show them all
            //the multiple servers will get shown later
            /*            if (availableServers.Count > 1)
                        {
                            //more than 1
                            for (int i = 0; i < availableServers.Count; i++)
                            {
                                ServerFunctions.SendData(player, $"Connecting to {ServerFunctions.servers[availableServers[i]].serverName} server!");

                            }
                        }*/
            //else
            if (availableServers.Count == 1)
            {
                //only 1
                chosenServer = availableServers[0];
                ServerFunctions.SendData(player, $"Connecting to {Config.config.gameName} server!");// ServerFunctions.servers[chosenServer].serverInfo.Name} server!");
                //find player in login connections
                for (int i = 0; i < ServerFunctions.servers.Count; i++)
                {
  //                  if (ServerFunctions.servers[i].serverType == "login")//we don't reference this script anymore anyway
    //                {
                        for (int j = 0; j < ServerFunctions.servers[i].connections.Count; j++)
                        {
                            if (ServerFunctions.servers[i].connections[j].player == player)
                            {
                                //add this player connection from login to connected game server
                                player.CurrentServer = ServerFunctions.servers[chosenServer];
//                                ServerFunctions.servers[i].logFile.WriteLine($"{player.IP} transfered from login server to {player.CurrentServer.serverInfo.Name}");
//                                player.CurrentServer.logFile.WriteLine($"{player.IP} transfered from login server to {player.CurrentServer.serverInfo.Name}");
                                player.CurrentServer.connections.Add(ServerFunctions.servers[i].connections[j]);
                                ServerFunctions.servers[i].connections.RemoveAt(j);
                                whichChoice = "character";
                            }
                        }
                  //  }
                }
            }

            if (availableServers.Count == 0)
            {//no servers in the list
                ServerFunctions.SendData(player, "There are currently no game servers available. Please try again later.");
                return;
            }
            ShowChoices(player);
        }

        public void ShowChoices(Player player)
        {
            switch (whichChoice)
            {
                case "server":
                    //this is for when more than 1 allowed server
                    //ServerFunctions.SendData(player, $"Connecting to {ServerFunctions.servers[availableServers[i]].serverName} server!");
                    ServerFunctions.SendData(player, "Available servers:<br>");
                    for (int i = 0; i < availableServers.Count; i++)
                    {
      //                  ServerFunctions.SendData(player, $"   {i + 1}) <link=\"choose{i + 1}\">{ServerFunctions.servers[availableServers[i]].serverInfo.Name}</link>");
                    }
                    break;
                case "character":
                    if (player.AccountType >= AccountType.Mod)//   loginAccount.AccountType == "admin" || loginAccount.AccountType == "mod")
                    {
                        ServerFunctions.SendData(player, "Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.");
                        ServerFunctions.SendData(player, "Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>");
                    }
                    //check for character login/creation
                    characterList = DB.DBAccess.GetCharacters(player, Config.config.gameName);//  chosenServer);//get the list of characters for this player on this server
                                                                                    //List<Character> characters = DB.DBAccess.GetCharacters(player, chosenServer);//get the list of characters for this player on this server

                    //   foreach (Character character in characters)
                    //   {
                    //       charGuid.Add(character.Id);
                    //   }
                    characterList.Add(new Character());
                    //charGuid.Add(Guid.Empty);
                    //.AccountID);//  GetCharListForAccount(loginAccount.AccountID);
                    //DataTable charList = DBAccess.GetCharListForAccount(loginAccount.AccountID);// accountID);
                    ServerFunctions.SendData(player, "Current characters:");
                    //           if (player.AccountType >= AccountType.Mod)
                    //           {//not sure about this, might take this out
                    //               SendData(player, "   (connect as no character)");//let admins and mods connect as no character in order to do behind-the-scenes stuff (room/item/npc creation, etc)
                    //           }
                    //             if (characters.Count > 0)//  charList.Rows.Count > 0)
                    //           {//current characters on this account
                    //SendData(clientGUID, "Current characters:");
                    if (characterList.Count == 1)
                    {//if this is only 1, that means the only character in the list is the empty new one we just put in
                        ServerFunctions.SendData(player, "   (no characters created)");
                        ServerFunctions.SendData(player, $"<br>   1) <link=\"choose 1\">Create new character</link><br>");
                    }
                    for (int i = 0; i < characterList.Count; i++)//  charList.Rows.Count; i++)
                    {
                        //ServerConsole.newConsole.WriteLine($"Name: {characterList[i].CharName}");
                        if (characterList[i].Id != Guid.Empty)
                        {
                            //string charName = DB.DBAccess.GetById<Character>(characterList[i].Id, DB.Collections.Character).CharName;
                            //ServerFunctions.SendData(player, $"   {i + 1}) a{characterList[i].CharName}");
                            ServerFunctions.SendData(player, $"   {i + 1}) <link=\"choose {i+1}\">a{characterList[i].Name}</link>");//  charList.Rows[i]["charName"]}");
                        } else
                        {
                            ServerFunctions.SendData(player, $"<br>   {i + 1}) <link=\"choose {i+1}\">Create new character</link><br>");
                        }

                    }
           //         }
         //           else
       //             {//no characters created on this account
     //                   ServerFunctions.SendData(player, "   (no characters created)");
   //                 }
     //               ServerFunctions.SendData(player, $"   {characters.Count + 1}) <link=\"newcharacter\">Create new character</link><br>");
                    break;
            }
        }

 //       public void Choose(Player player, string[] command)
 //       {
 //           int menuOption = -1;
            //since textmeshpro link ids can (as far as I can tell) use spaces, we'll use the link ids as 'commands' sending back for basic stuff
            //'choose' and the like.
            //this is to get the choice # from either typing 'choose 1' or clicking a link 'choose1'
            //moved this to command-choose since each tempScript could be different
            /*          if (command.Length > 1)
                      {
                          choice = command[1];
                      }
                      if (command[0].Length > 6)
                      {
                          choice = command[0].Remove(0, 6);
                      }*/
/*
            if (command.Length > 1)
            {
                if (!Int32.TryParse(command[1], out menuOption))
                {
                    ServerFunctions.SendData(player, "Invalid selection! Please enter the number for the option you want.");
                    return;
                }
            }

            //          if (Int32.TryParse(Console.ReadLine(), out menuOption))
            //          {
            //ServerFunctions.SendData(player, $"picked {menuOption}");
            switch (whichChoice)
            {
                case "server":
                    break;
                case "character":
                    if (menuOption > 0 && menuOption <= characterList.Count)
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
                            player.tempScript = null;
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
                            Room charGen = DB.DBAccess.GetById<Room>(ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                            //Region charReg;
                            if (charGen.Region != Guid.Empty)//if the room has an associated region to it
                            {
                                Region charReg = DB.DBAccess.GetById<Region>(charGen.Region, DB.Collections.Region);
                                if (charReg != null && charReg.IsInstanced)//if we found a region for this room and it is an instanced region
                                {//do stuff
                                    ServerFunctions.SendData(player, "this is a region instance");

                                    break;
                                } else
                                {
                                    //if we didn't find a region for this room, or it is not an instanced region
                                    ServerFunctions.SendData(player, "not instanced");
                                }
                            } else
                            {
                                ServerFunctions.SendData(player, "no region");
                            }
                            //ServerFunctions.SendData(player, "no region,or not instanced");
                            player.CurrentRoom = ServerFunctions.servers[chosenServer].firstRoom;
                            //player.tempScript = null;
                            player.tempScript = new CharManager(player, player);
                            ServerFunctions.AddCommandToQueue(player, "look");
                        }
                    }
                    else
                    {
                        ServerFunctions.SendData(player, "Invalid selection! Please enter the number for the option you want.");
                    }
                    break;
            }

            //          } else
            //          {
            //parse failed
            //          }




        }*/

        //List<int> availableServers = new List<int>();//put default/allowed servers into here so we can check

        /*          for (int i = 0; i<servers.Count; i++)
                  {
                      //ignore login server
                      if (servers[i].serverType == "game")
                      {
                          if (servers[i].serverIsRunning && (player.AccountType >= AccountType.Mod || servers[i].isDefault))
                          {
                              //if this server is up AND
                              //either player is mod/admin or this server is set as a default server
                              //then add it to the list
                              availableServers.Add(i);
                              break;
                          }
                          //if this server is not default, check for if it's up and go through explicit allowed servers on this player
                          if (servers[i].serverIsRunning && !servers[i].isDefault)
                          {
                              string temp = DB.DBAccess.GetInfo(player, "allowedServers");
          //if (string.IsNullOrEmpty(temp))//I think I fixed this in the GetInfo method
          //{//if the allowedServers is null (because liteDB sets empty strings to null)
          //  temp = "";//then set the temp to empty string so the split doesn't choke on the null
          // }
          string[] allowed = temp.Split(',', StringSplitOptions.RemoveEmptyEntries);

                              for (int j = 0; j<allowed.Length; j++)
                              {
                                  if (allowed[j] == servers[i].serverName)
                                  {
                                      availableServers.Add(i);
                                      break;
                                  }
                              }
                          }
                      }
                  }*/
        /*
                    //if there is only 1 server in this list, then just automatically connect to it, if more than 1 show them all
                    if (availableServers.Count > 1)
        {
            //more than 1
            for (int i = 0; i < availableServers.Count; i++)
            {
                SendData(player, $"Connecting to {servers[availableServers[i]].serverName} server!");

            }
        }
        else if (availableServers.Count == 1)
        {
            //only 1
            SendData(player, $"Connecting to {servers[availableServers[0]].serverName} server!");
            //find player in login connections
            for (int i = 0; i < servers.Count; i++)
            {
                if (servers[i].serverType == "login")
                {
                    for (int j = 0; j < servers[i].connections.Count; j++)
                    {
                        //add this player connection from login to connected game server
                        servers[availableServers[0]].connections.Add(servers[i].connections[j]);
                        servers[i].connections.RemoveAt(j);
                    }
                }
            }
        }
        else
        {//no servers in the list
            SendData(player, "There are currently no game servers available. Please try again later.");
        }
        */
        /*
        if (player.AccountType >= AccountType.Mod)//   loginAccount.AccountType == "admin" || loginAccount.AccountType == "mod")
        {
            SendData(player, "Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.");
            SendData(player, "Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>");
        }
            */
        //check for valid servers they can connect to/allowed to
        //           string temp = DB.DBAccess.GetInfo(player, "allowedServers");
        //           if (string.IsNullOrEmpty(temp))
        //           {//if the allowedServers is null (because liteDB sets empty strings to null)
        //               temp = "";//then set the temp to empty string so the split doesn't choke on the null
        //           }
        //           string[] allowed = temp.Split(',', StringSplitOptions.RemoveEmptyEntries);// DB.DBAccess.GetInfo(player, "allowedServers").Split(',', StringSplitOptions.RemoveEmptyEntries);

        //         int allowedNum = 0;
        //       if (player.AccountType < AccountType.Mod)
        //     {//if this is a normal player, not admin or mod, let's see how many servers they're allowed on
        //       allowedNum = allowed.Length;
        // }
        /*       if (player.AccountType < AccountType.Mod && allowed.Length == 1)
               {
                   //if this is a normal player and they're only allowed on one server, let's just automatically log into that one
                   foreach(ServerMain server in servers)
                   {
                       if (server.serverName == allowed[0] && server.serverIsRunning)
                       {
                           //SendData(player, $"logging into {server.serverName}");
                       }
                   }
               } else
               {
                   foreach (ServerMain server in servers)
                   {
                       if (server.serverType == "game")
                       {
                           //only interested in the game servers, not login
                           if (player.AccountType >= AccountType.Mod && server.serverIsRunning)
                           {//if admin or mod, then yes allowed
                               SendData(player, $"{server.serverName}");
                           }
                           else
                           {//normal player
                               foreach (string s in allowed)
                               {
                                   if (s == server.serverName && server.serverIsRunning)
                                   {//if this server name is in their allowed list
                                       SendData(player, $"{server.serverName}");
                                   }
                               }
                           }
                       }
                   }
               }*/

        /*
    //check for character login/creation
    List<Character> characters = DB.DBAccess.GetCharacters(player.AccountID);//  GetCharListForAccount(loginAccount.AccountID);
                                                                             //DataTable charList = DBAccess.GetCharListForAccount(loginAccount.AccountID);// accountID);
    SendData(player, "Current characters:");
    //           if (player.AccountType >= AccountType.Mod)
    //           {//not sure about this, might take this out
    //               SendData(player, "   (connect as no character)");//let admins and mods connect as no character in order to do behind-the-scenes stuff (room/item/npc creation, etc)
    //           }
    if (characters.Count > 0)//  charList.Rows.Count > 0)
    {//current characters on this account
     //SendData(clientGUID, "Current characters:");
        for (int i = 0; i < characters.Count; i++)//  charList.Rows.Count; i++)
        {
            //connections[connectionIndex].account.Characters.Add(characters[i].CharacterID);//not sure I need this?
            SendData(player, $"   {i + 1}) {characters[i].CharName}");//  charList.Rows[i]["charName"]}");
        }
    }
    else
    {//no characters created on this account
        SendData(player, "   (no characters created)");
    }
    SendData(player, $"   {characters.Count + 1}) <link=\"newcharacter\">Create new character</link><br>");


        }*/
    }
}
