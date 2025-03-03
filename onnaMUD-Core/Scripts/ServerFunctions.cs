using onnaMUD.Characters;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
using onnaMUD.Database;
//using onnaMUD.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//using static onnaMUD.MUDServer.ServerMain;
using onnaMUD.Utilities;
using onnaMUD.Temps;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using static onnaMUD.MUDServer.ServerMain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using onnaMUD.Settings;
using static onnaMUD.Utilities.ServerConsole;
using System.Security.Policy;
//using static onnaMUD.Program;

namespace onnaMUD.MUDServer
{
    public static class ServerFunctions
    {
        static public List<ServerMain> servers = new List<ServerMain>();
        static public ServerMain? server;
        static public int numOfBytes = 1024;

        /// <summary>
        /// Standard output to the user frontend (main window)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dataToSend"></param>
        //       public void SendData(TcpClient client, string dataToSend)
        //     {
        //SendData(client, "110", dataToSend);
        //   }
        
        /// <summary>
        /// Standard output to the user frontend (main window)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="dataToSend"></param>
        /// <param name="sendCaret">send '>' ?</param>
        public static void SendData(this Player player, string dataToSend, bool sendCaret = true)
        {
            if (!sendCaret)
            {
                SendData(player, "110", dataToSend, false);//  "<br>"+dataToSend);
            }
            else
            {
                SendData(player, "110", dataToSend);//  "<br>"+dataToSend);
            }
        }


  //      public static void SendData(Player player, string msgCode, string dataToSend)
    //    {
      //      SendData(player.Client, msgCode, dataToSend);
       // }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msgCode"></param>
        /// <param name="dataToSend"></param>
        /// <param name="sendCaret">send '>' ?</param>
        public static void SendData(this Player player, string msgCode, string dataToSend, bool sendCaret = true)// TcpClient client, string msgCode, string dataToSend)//  Guid senderGuid, string msgCode, string dataToSend)
        {
            //do we actually NEED to send the server guid everytime?
            //logFile.WriteLine(msgCode + " " + dataToSend);
            //make an array for all the characters we might need to escape out in the message
            string[] badCharacters = { ":", "<", ">" };
            NetworkStream? clientStream = null;
            //int serverIndex = -1;
            try
            {
                clientStream = player.Client.GetStream();
            }
            catch (Exception)
            {
                for (int i = 0; i < servers.Count; i++)
                {
                    for (int j = 0; j < servers[i].connections.Count; j++)
                    {
                        if (servers[i].connections[j].player == player)
                        {
                            servers[i].logFile.WriteLine($"Error with getting NetworkStream for {player.AccountID}. Connection issue?");
                        }
                    }
                }
                return;
            }
            int tempIndex = 0;
            string indexString = tempIndex.ToString("D5");

            //if we're sending data from a server (or receiving data from a server, not client) then checking for invalid data is not needed?
            //since we already know we're not sending bad data
            //               if (dataToSend.IndexOf("::") == -1 && dataToSend.IndexOf("<EOF>") == -1)
            //               {//if the message we're sending doesn't have the delimiter or EOF in it, we're good and send it

            if (sendCaret && msgCode == "110")
            {
                dataToSend = $"{dataToSend}<br>>";
            }

            string msgString = indexString + "::" + msgCode + "::" + dataToSend + "<EOF>";//removed serverguid
            //Console.WriteLine(msgString);
            tempIndex = msgString.IndexOf("<EOF>");
            indexString = tempIndex.ToString("D5");
            msgString = indexString + "::" + msgCode + "::" + dataToSend + "<EOF>";//removed serverguid
            //Console.WriteLine(msgString);

            byte[] msg = Encoding.ASCII.GetBytes(msgString);
            try
            {
                clientStream.Write(msg);
            }
            catch (Exception)
            {
                for (int i = 0; i < servers.Count; i++)
                {
                    for (int j = 0; j < servers[i].connections.Count; j++)
                    {
                        if (servers[i].connections[j].player == player)
                        {
                            servers[i].logFile.WriteLine($"Error with writing to NetworkStream for {player.AccountID}. Connection issue?");
                        }
                    }
                }
                return;
            }
        }

 /*       public static void NewTrialAccount(NewTrial newTrial, string option)
        {
            switch (option)
            {
                case "add":
                    servers[0].newTrialAccounts.Add(newTrial);
                    servers[0].logFile.WriteLine("player added");
                    break;
                case "remove":
                    for (int i = 0; i < servers[0].newTrialAccounts.Count; i++)
                    {
                        if (servers[0].newTrialAccounts[i] == newTrial)
                        {
                            servers[0].newTrialAccounts.RemoveAt(i);
                        }
                    }
                    break;
            }

        }*/

  /*      public static NewTrial NewTrialAccount(Player player)
        {
            NewTrial newTrial = new NewTrial();
            if (servers[0].newTrialAccounts.Count > 0)
            {
                for (int i = 0; i < servers[0].newTrialAccounts.Count; i++)
                {
                    if (servers[0].newTrialAccounts[i].player == player)// clientGUID == clientGuid)
                    {
                        //SendData(player, "matched in newtrial");
                        return servers[0].newTrialAccounts[i];
                    }
                }
             //   return new NewTrial();
            }// else
            //{
            //SendData(player, "didn't match in new trial");
            //NewTrial temp = new NewTrial();
            newTrial.player = player;
            return newTrial;
            //}

        }*/

        //this checks the current list of active newTrial players going through the new trial account process
/*        public static bool IsTrialProcessStarted(Player player)// Guid clientGuid)
        {
            //since servers[0] is the login server, it's the only one that will have the newTrial stuff
            NewTrial temp = new NewTrial();

            if (servers[0].newTrialAccounts.Count > 0)
            {
                for (int i = 0; i < servers[0].newTrialAccounts.Count; i++)
                {
                    if (servers[0].newTrialAccounts[i].player == player)// clientGUID == clientGuid)
                    {
                        return true;
                    }
                }
               // return false;
            }
            //else
           // {//no trial account processes currently going
                return false;
          //  }
        }*/

        public static async void DoLogin(Player player)
        {
            //Player newConnection = new Player();
            //check if guid is used already?
            player.connectionStatus = Player.ConnectionStatus.Connecting;
//            player.Guid = Guid.NewGuid();
            //newConnection.Client = incomingClient;
//            player.IP = player.Client.Client.RemoteEndPoint.ToString();//  incomingClient.Client.RemoteEndPoint.ToString();
//            player.ClientTask = ConnectedClient(player);

//            ServerMain.Connections tempConn = new ServerMain.Connections();

//            tempConn.player = player;
            //tempConn.clientTask = ConnectedClient(player);// incomingClient, newConnection);// tempConn.guid);
//            servers[0].connections.Add(tempConn);
            //send the created session guid to the player
//            player.SendData("052", $"guid::{player.Guid.ToString()}");
            //SendData(player, "052", $"guid::{player.Guid.ToString()}");
            //when this is sent, player sends back account name and password

            //check for account login/trial account creation
            //also check the client itself on player if it's still there so we don't get stuck in a infinite loop waiting for something that will never happen
            bool isAccount = false;
            while (!isAccount)
            {
                if (IsClientStillConnected(player))
                {
                    //SendData(player, "checking.");
                    if (player.AccountID == Guid.Empty)
                    {
                        //SendData(player, "waiting for accountID");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        isAccount = true;
                    }
                } else
                {
                    //client is disconnected for whatever reason
                    return;
                }
            }
            //get the account name and send to player?
            //SendToLogfile(player, $"{player.IP} account logged in!");//maybe add accountName to player?
            player.SendData("<br>Logged in!", false);
            //SendData(player, "<br>Logged in!", false);//adding an additional br to the one we always add on the front of output so we skip a line
            //FYI: already did the 052 message for the account name in ServerMain - ProcessMessage 050 for user login
            //Console.WriteLine("testing...");
//            player.tempScript = new CharSelect(player);

            //do the initial check for servers here so there is a starting point for the choose command to work from
   //         List<ServerMain> showServers = ListAvailableServers(player);
   //         if (showServers.Count == 0)
   //         {
   //             SendData(player, "<br>There are currently no game servers available. Please try again later.");

                //disconnect from here?
   //             return;
   //         } else// if (showServers.Count == 1)
   //         {
                ShowCharSelection(player);
                //chosenServer = availableServers[0];
//                SendData(player, $"<br>Connecting to {showServers[0].serverInfo.Name} server!<br>", false);
                //find player in login connections

                //NEED TO REDO THIS!!!!!!!
//                for (int i = 0; i < servers.Count; i++)
//                {
  //                  if (servers[i].serverType == "login")
  //                  {
//                        for (int j = 0; j < servers[i].connections.Count; j++)
//                        {
                            //find the player in the connections list on the login server
//                            if (servers[i].connections[j].player == player)
//                            {
                                //add this player connection from login to connected game server
//                                player.CurrentServer = showServers[0];
//                                servers[i].logFile.WriteLine($"{player.IP} transfered from login server to {player.CurrentServer.serverInfo.Name}");
//                                player.CurrentServer.logFile.WriteLine($"{player.IP} transfered from login server to {player.CurrentServer.serverInfo.Name}");
                                //copy the connections entry from the login server to the selected game server
//                                player.CurrentServer.connections.Add(servers[i].connections[j]);
                                //remove the connections entry from the login server
//                                servers[i].connections.RemoveAt(j);
                                //from here, show character list options/new character
                                //whichChoice = "character";
//                                ShowCharSelection(player);
/*                                List<Character> characterList = DB.DBAccess.GetCharacters(player, player.CurrentServer.serverName);

                                if (player.AccountType >= AccountType.Mod)//   loginAccount.AccountType == "admin" || loginAccount.AccountType == "mod")
                                {
                                    SendData(player, "Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.");
                                    SendData(player, "Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>");
                                }

                                SendData(player, "Current characters:");
                                if (characterList.Count == 0)
                                {//if 0 characters, then only option is to make a new one
                                    SendData(player, "   (no characters created)");
                                    SendData(player, $"<br>   1) <link=\"choose 1\">Create new character</link><br>");
                                }
                                else
                                {
                                    for (int i = 0; i < characterList.Count; i++)//  charList.Rows.Count; i++)
                                    {
                                        if (characterList[i].Id != Guid.Empty)
                                        {
                                            SendData(player, $"   {i + 1}) <link=\"choose {i + 1}\">a{characterList[i].CharName}</link>");//  charList.Rows[i]["charName"]}");
                                        }
                                        else
                                        {
                                            SendData(player, $"<br>   {i + 1}) <link=\"choose {i + 1}\">Create new character</link><br>");
                                        }
                                    }
                                }*/
      //                      }
      //                  }
  //                  }
  //              }
 //           }// else //more than 1 available server for this player
           // {
             //   SendData(player, "<br>Available servers:<br>");
               // for (int i = 0; i < showServers.Count; i++)
               // {
                 //   SendData(player, $"   {i + 1}) <link=\"choose {i + 1}\">{showServers[i].serverInfo.Name}</link>");
               // }
           // }


            //do all this stuff in CharSelect
            //check for available servers, allowed, etc...
 /*           List<int> availableServers = new List<int>();//put default/allowed servers into here so we can check

            for (int i = 0; i < servers.Count; i++)
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

                        for (int j = 0; j < allowed.Length; j++)
                        {
                            if (allowed[j] == servers[i].serverName)
                            {
                                availableServers.Add(i);
                                break;
                            }
                        }
                    }
                }
            }

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


            if (player.AccountType >= AccountType.Mod)//   loginAccount.AccountType == "admin" || loginAccount.AccountType == "mod")
            {
                SendData(player, "Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.");
                SendData(player, "Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>");
            }

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
            if (player.AccountType < AccountType.Mod && allowed.Length == 1)
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
            }


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

            //ServerFunctions.SendData(player, "090", "character1,Blahblahmoomoomoo");
            //ServerFunctions.SendData(player, "<link=\"character1\"></link>");
//            SendData(player, "Doh<link=\"character1\">Blahblahmoomoomoo</link>blah");*/
        }

        public static void UpdateCharOnPlayer(Player player, Character updatedChar)
        {
            var playerT = player.GetType();//get class type for player
            var charT = updatedChar.GetType();//get type for character class to get field/property
            foreach (var sourceF in charT.GetProperties())//  GetFields())//get all fields from character class type
            {
                //ServerConsole.newConsole.WriteLine($"{sourceF.Name}");
                var playerF = playerT.GetProperty(sourceF.Name);//  GetField(sourceF.Name, BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                                                                //ServerConsole.newConsole.WriteLine($"Name: {playerF}");
                if (playerF == null)//if no matching field/property name from character class, skip and go to next
                    continue;
                playerF.SetValue(player, sourceF.GetValue(updatedChar, null), null);//set value of character property to player property
            }
        }

        public static object UpdateObject(object objectToUpdate, object updatedSourceObject)
        {
            var updatingT = objectToUpdate.GetType();//get class type for thing we're updating
            var sourceT = updatedSourceObject.GetType();//get class type for updated thing

            foreach (var sourceF in sourceT.GetProperties())//get all fields/properties/variables from source type
            {
                //get the next variable/field from updated source type and see if it has a match in the class type to be updated
                var updatingF = updatingT.GetProperty(sourceF.Name);

                //if the 2 fields/variables from the source & updating classes don't match, skip it
                if (updatingF == null)
                    continue;
                //if there is a match, updated the variable from the source to the updating
                updatingF.SetValue(objectToUpdate, sourceF.GetValue(updatedSourceObject, null), null);
            }
            return objectToUpdate;
        }

        public static Player GetPlayer(Character character)
        {
            Player tempPlayer = new Player();
            for (int i = 0; i < servers.Count; i++)
            {
                for (int j = 0; j < servers[i].connections.Count; j++)
                {
             //       if (servers[i].connections[j].player.character == character)
               //     {
                 //       return servers[i].connections[j].player;
                   // }
                }
            }
            return tempPlayer;
        }

        public static bool IsCharNameAllowed(Player player, string charName)
        {
            //do checks for name
            if (charName.Length < 4 || charName.Length > 16)
            {
                //character name is too short or too long
                player.SendData("Character name is too short/long. Length of names allowed: 4-18 characters");
                //SendData(player, "Character name is too short/long. Length of names allowed: 4-18 characters");
                return false;
            }
            string[] invalidChars = { ",", ".", "-", "'", "\"", "@" };

            for (int i = 0; i < invalidChars.Length; i++)
            {
                if (!charName.All(Char.IsAsciiLetter))//  charUpdate.CharName.IndexOf(invalidChars[i]) > 0)
                {
                    player.SendData("Character name has invalid characters. Alphabetic letters only please.");
                    //SendData(player, "Character name has invalid characters. Alphabetic letters only please.");
                    return false;
                }
            }
            //check if name is already taken
            if (DB.DBAccess.IsUsedCharName(charName))
            {
                //name is already taken
                player.SendData("Character name is already in use. Please choose a different name.");
                //SendData(player, "Character name is already in use. Please choose a different name.");
                return false;
            }

            if (player.AccountType >= AccountType.Admin)
            {
                //allow admins to bypass forbidden names list check in order to set names manually?

            }
            else
            {
                //check if name is not allowed by forbidden names list

            }
            //if we got to this point, then name is allowed to be used
            return true;
        }

        public static void ShowCharSelection(Player player)
        {
            List<Character> characterList = DB.DBAccess.GetCharacters(player, Config.config.gameName);
            string outputString = "";

            if (player.AccountType >= AccountType.Mod)//   loginAccount.AccountType == "admin" || loginAccount.AccountType == "mod")
            {
                outputString += "<br>Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.";
 //               outputString += "<br>Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>";
                //SendData(player, "Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.");
                //SendData(player, "Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>");
            }

            outputString += "<br>Current characters:<br>";
            //SendData(player, "Current characters:");
            if (characterList.Count == 0)
            {//if 0 characters, then only option is to make a new one
                outputString += $"   (no characters created)<br>   1) <link=\"clicklink charselect 0\">Create new character</link><br>";
                //SendData(player, "   (no characters created)");
                //SendData(player, $"<br>   1) <link=\"clicklink charselect 0\">Create new character</link><br>");
            }
            else
            {
                for (int i = 0; i < characterList.Count; i++)//  charList.Rows.Count; i++)
                {
                    if (characterList[i].Id != Guid.Empty)
                    {
                        //was link=\"choose {i + 1}\"
                        outputString += $"   {i + 1}) <link=\"clicklink charselect {i}\">{characterList[i].Name}</link>";
                        //SendData(player, $"   {i + 1}) <link=\"clicklink charselect {i}\">{characterList[i].Name}</link>");//  charList.Rows[i]["charName"]}");
                    }
                    else
                    {
                        outputString += $"<br>   {i + 1}) <link=\"clicklink charselect {i}\">Create new character</link><br>";
                        //SendData(player, $"<br>   {i + 1}) <link=\"clicklink charselect {i}\">Create new character</link><br>");
                    }
                }
            }
            player.SendData(outputString);
            //SendData(player, outputString);
        }

        public static List<ServerMain> ListAvailableServers(Player player)
        {
            List<ServerMain> tempList = new List<ServerMain>();

            for (int i = 0; i < servers.Count; i++)
            {
                //ignore login server
  //              if (servers[i].serverType == "game")
  //              {
                    if (servers[i].serverIsRunning && (player.AccountType >= AccountType.Mod))// || servers[i].serverInfo.IsDefault))
                    {
                        //if this server is up AND
                        //either player is mod/admin or this server is set as a default server
                        //then add it to the list
                        tempList.Add(servers[i]);
                        //availableServers.Add(i);
                        break;
                    }
                    //if this server is not default, check for if it's up and go through explicit allowed servers on this player
                    if (servers[i].serverIsRunning)// && !servers[i].serverInfo.IsDefault)
                    {
                        string temp = DB.DBAccess.GetInfo(player, "allowedServers");
                        string[] allowed = temp.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        for (int j = 0; j < allowed.Length; j++)
                        {
                            if (allowed[j] == Config.config.gameName)
                            {
                                tempList.Add(servers[i]);
                                //availableServers.Add(i);
                                break;
                            }
                        }
                    }
 //               }
            }
            return tempList;
        }

        private static async Task ConnectedClient(Player player)// TcpClient client, Player player)// Guid clientGUID)//this is for connecting clients on either login or game servers, any connected client
        {
            //string[] charsToEscape;
            //Console.WriteLine("Client accepted!");
            NetworkStream clientStream = player.Client.GetStream();
            string receivedDataBuffer = "";
            byte[] bytes = new byte[numOfBytes];
            bool checkMessage = false;
            int eofIndex = 0;

            //Console.WriteLine(client.Client.LocalEndPoint);//this SHOULD be the ip of the client, from their end

            while (true)
            {
                try
                {
                    //Console.WriteLine("waiting");
                    int bytesRec = await clientStream.ReadAsync(bytes, 0, numOfBytes);
                    if (bytesRec > 0)
                    {
                        receivedDataBuffer += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        checkMessage = true;
                    }
                    else
                    {
//                        logFile.WriteLine($"{player.IP} client disconnected.");
                        player.Client.Close();
                        //RemoveConnection(client);
                        return;
                    }
                }
                /*              catch (SocketException e)
                              {
                                  //Console.WriteLine(i);
                                  Console.WriteLine(e.ToString());
                                  //Thread.Sleep(1000);
                                  return;// false;
                                         //throw;
                              }
                              catch (IOException)// ioe)
                              {
                                  logFile.WriteLine($"Client {player.IP} no longer able to be read! Disconnecting.");
                                  clientStream.Dispose();
                                  player.Client.Close();
                                  //Thread.Sleep(1000);
                                  return;
                              }*/
                catch (Exception)
                {
                    //Console.WriteLine(doh.ToString());
//                    logFile.WriteLine($"Client {player.IP} no longer able to be read! Disconnecting.");
                    //clientStream.Dispose();
                    player.Client.Close();
                    //Thread.Sleep(1000);
                    return;
                }

                while (checkMessage)
                {
                    //logFile.WriteLine(receivedDataBuffer);
                    //if we've received data into our buffer, check for message length then process
                    if (receivedDataBuffer.IndexOf("::") == 5)//was -1 (index is somewhere in string), now is 5 to make sure index is in the right place since the index of :: should be 5
                    {//if we found the first set of :: means that we've gotten the index int for <EOF>
                        try
                        {
                            eofIndex = Int32.Parse(receivedDataBuffer.Substring(0, 5));//parse the first 5 characters for the index of EOF
                        }
                        catch (FormatException)
                        {
                            SendToLogfile(player, $"{player.AccountID} has an invalid message index. Clearing data buffer.");
                            //                     try
                            //                   {
                            //                     GetCurrentServer(player).logFile.WriteLine($"{player.Guid} has an invalid message index. Clearing data buffer.");
                            //               }
                            //             catch (NullReferenceException)
                            //           {
                            //             Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? int32.Parse in ConnectedClient");
                            //       }
                            player.SendData("There was an communications error. Please try again.");
                            //SendData(player, "There was an communications error. Please try again.");
                            checkMessage= false;
                            receivedDataBuffer = "";
                            //return;
                        }

                        if (eofIndex > 0 && receivedDataBuffer.Length >= eofIndex + 4)//if we got the index correctly and we've got the whole message
                        {
                            string firstMessage = receivedDataBuffer.Substring(0, eofIndex + 5);
                            firstMessage = firstMessage.Substring(7);//this basically removes the EOF index and first :: from the beginning of the message as we don't need it anymore
                            firstMessage = firstMessage.Remove(firstMessage.IndexOf("<EOF>"));
                            receivedDataBuffer = receivedDataBuffer.Remove(0, eofIndex + 5);

                            //logFile.WriteLine(firstMessage);
                            //Console.WriteLine(receivedDataBuffer.Length);
                            //send the first whole message to process, then trim that out
                            //Console.WriteLine(receivedDataBuffer);

                            //send the message to ProcessMessage on specific server, then from there add to queue if we need to
                            //sends the message to the ProcessMessage here on ServerFunctions, which finds the server the player is on
                            ProcessMessage(player, firstMessage);
                            //try
                            //{
                              //  GetCurrentServer(player).ProcessMessage(player, firstMessage);//  commandQueue.Add(command);
                            //}
                           // catch (NullReferenceException)
                           // {
                             //   Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? add to commandQueue in ConnectedClient");
                           // }
                            //move this bit to ServerMain.DoCommand and only add to the queue the commands from the player/otherwise
//                            ServerMain.CommandQueue command = new ServerMain.CommandQueue();
//                            command.player = player;//  clientGuid = clientGUID;
//                            command.commandMessage = firstMessage;
//                            try
//                            {
//                                GetCurrentServer(player).commandQueue.Add(command);
//                            }
//                            catch (NullReferenceException)
//                            {
//                                Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? add to commandQueue in ConnectedClient");
//                            }
                            //ProcessMessage(clientGUID, firstMessage);// receivedDataBuffer.Substring(0, eofIndex + 5));//second variable of substring is length, not index
                            //receivedDataBuffer = receivedDataBuffer.Remove(0, eofIndex + 5);//second variable of remove is also length, not index since <EOF> is 5 char long
                            //Console.WriteLine(receivedDataBuffer);
                            //Console.WriteLine("message sent to process, should now wait for another?");

                        }
                        else
                        {
                            //Console.WriteLine("another doh?");
                            checkMessage = false;
                        }

                    }
                    else
                    {
                        checkMessage = false;
                        /*       Console.WriteLine(receivedDataBuffer.IndexOf("::"));
                               if (receivedDataBuffer.Length >= 7)//if we've hit 7 and not found ::, then we've not gotten a correct message
                               {
                                   logFile.WriteLine("Incorrect message protocol!");
                                   logFile.WriteLine(receivedDataBuffer);
                                   //clear the buffer and start over?
                                   receivedDataBuffer = "";
                                   SendData(client, serverGuid, "110", "Try try again.");
                               }
                               else
                               {
                                   //we've gotten data but we haven't gotten to 7 characters yet
                                   //client already disconnected?
                                   Console.WriteLine("doh?");
                               }*/
                    }
                }
            }
        }

        public static void SendReadyCursor(Player player)
        {
            //if (player.Roundtime)


        }

        /// <summary>
        /// Finds server player is currently on and adds the command to the queue for that server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        public static void AddCommandToQueue(Player player, string command)
        {
            if (player.CurrentServer != null)
            {
                //SendData(player, player.CurrentServer.commandQueue.Count.ToString());
                ServerMain.CommandQueue commandTyped = new ServerMain.CommandQueue();
                commandTyped.player = player;
                commandTyped.commandMessage = command;
                player.CurrentServer.commandQueue.Add(commandTyped);
                //SendData(player, player.CurrentServer.commandQueue.Count.ToString());
                //return;
            } else
            {
                Console.WriteLine($"Somehow, {player.AccountID} does not have a current server set. Maybe we need to check on this? ServerFunctions.AddCommandToQueue");
            }

          /*  for (int i = 0; i < servers.Count; i++)
            {
                for (int j = 0; j < servers[i].connections.Count; j++)
                {
                    if (servers[i].connections[j].player == player)
                    {
                        CommandQueue commandTyped = new CommandQueue();
                        commandTyped.player = player;
                        commandTyped.commandMessage = command;
                        servers[i].commandQueue.Add(commandTyped);
                        return;
                    }
                }
            }
            //if we got to this point, couldn't find player on a server
            Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? ServerFunctions.AddCommandToQueue");
            */
        }

        /// <summary>
        /// Finds server player is currently on and processes the message code
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void ProcessMessage(Player player, string message)
        {
            if (player.CurrentServer != null)
            {
                player.CurrentServer.ProcessMessage(player, message);
            } else
            {
                Console.WriteLine($"Somehow, {player.AccountID} does not have a current server set. Maybe we need to check on this? ServerFunctions.ProcessMessage");
            }

      /*      for (int i = 0; i < servers.Count; i++)
            {
                for (int j = 0; j < servers[i].connections.Count; j++)
                {
                    if (servers[i].connections[j].player == player)
                    {
                        servers[i].ProcessMessage(player, message);
                        //CommandQueue commandTyped = new CommandQueue();
                        //commandTyped.player = player;
                        //commandTyped.commandMessage = command;
                        //servers[i].commandQueue.Add(commandTyped);
                        return;
                    }
                }
            }
            //if we got to this point, couldn't find player on a server
            Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? ServerFunctions.ProcessMessage");
            //ServerConsole.newConsole.WriteLine(message);*/
        }

        /// <summary>
        /// Finds server player is on and sends message to logfile
        /// </summary>
        /// <param name="player"></param>
        /// <param name="logMessage"></param>
        public static void SendToLogfile(Player player, string logMessage)
        {
            if (player.CurrentServer != null)
            {
                player.CurrentServer.logFile.WriteLine(logMessage);
            } else
            {
                Console.WriteLine($"Somehow, {player.AccountID} does not have a current server set. Maybe we need to check on this? ServerFunctions.SendToLogfile");
            }

          /*  for (int i = 0; i < servers.Count; i++)
            {
                for (int j = 0; j < servers[i].connections.Count; j++)
                {
                    if (servers[i].connections[j].player == player)
                    {
                        servers[i].logFile.WriteLine(logMessage);
                        return;
                    }
                }
            }
            //if we got to this point, couldn't find player on a server
            Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this? ServerFunctions.SendToLogfile");
            */
        }

        public static Thing? MatchObjectInRoom(Player player, Room room, string thingToMatch)
        {
            for (int i = 0; i < room.StuffInRoom.Count; i++)
            {
                if (room.StuffInRoom[i].ThingType == ThingType.Character)
                {
                    if (room.StuffInRoom[i].Name.ToLower().StartsWith(thingToMatch.ToLower()))
                    {
                        return (Character)room.StuffInRoom[i];
                        //ServerFunctions.SendData(player, $"You reach out and touch {((Character)room.Stuff[i]).CharName}.");
                        //return;
                    }
                    //ServerFunctions.SendData(player, $"{((Character)room.Stuff[i]).CharName}");
                    //ServerFunctions.SendData(player, $"{room.Stuff[i].CharName}")
                }

            }


            return null;
        }

        public static Room GetRoom(Player player, bool baseRoom = false)
        {
            if (player.CurrentServer != null)
            {
                //SendData(player, player.CurrentRoom.ToString());
                if (player.character.CurrentRoom == Guid.Empty)
                {
                    //player is not in a room at the moment, at character select?
                    return new Room();
                }

                if (player.RegionInstance != Guid.Empty)
                {
                    //if the player is in a region instance, get the room from that instance
                    for (int i = 0; i < player.CurrentServer.regions.Count; i++)
                    {
                        //find the instance that this player is in?
                        if (player.CurrentServer.regions[i].regionInstance == player.RegionInstance)
                        {
                            for (int j = 0; j < player.CurrentServer.regions[i].rooms.Count; j++)
                            {
                                //find the room in this instance that the player in is
                                if (player.CurrentServer.regions[i].rooms[j].Id == player.character.CurrentRoom)
                                {
                                    //SendToLogfile(player, "region room");
                                    return player.CurrentServer.regions[i].rooms[j];
                                }
                            }
                        }
                    }
                }
                else
                {
                    //player is not in a region instance, so just get the room from the cache that everybody can get to
                    for (int i = 0; i < player.CurrentServer.roomCache.Count; i++)
                    {
                        if (player.CurrentServer.roomCache[i].Id == player.character.CurrentRoom)
                        {
                            //SendToLogfile(player, "cache room");
                            return player.CurrentServer.roomCache[i];
                        }
                    }
                }
                SendToLogfile(player, $"{player.character.Name} somehow not found in GetRoom on {Config.config.gameName}!");
                return new Room();
            } else
            {
                Console.WriteLine($"Somehow, {player.AccountID} does not have a current server set. Maybe we need to check on this? ServerFunctions.GetRoom");
                return new Room();
            }
        }

        public static Room GetRoom(Player player, Guid roomID)
        {
            for (int i = 0; i < player.CurrentServer.roomCache.Count; i++)
            {
                if (player.CurrentServer.roomCache[i].Id == roomID)
                {
                    //SendToLogfile(player, "cache room");
                    return player.CurrentServer.roomCache[i];
                }
            }
            //somehow didn't find the room in the cache?
            SendToLogfile(player, $"{player.character.Name} somehow not found in GetRoom on {Config.config.gameName}!");
            return new Room();
        }

        /// <summary>
        /// Returns the index in ServerFunctions.servers of current server that player is on
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetCurrentServer(Player player)
        {
            //int index = -1;
            for (int i = 0; i < servers.Count; i++)
            {
                for (int j = 0; j < servers[i].connections.Count; j++)
                {
                    if (servers[i].connections[j].player == player)
                    {
                        //return servers[i];
                        //index = i;
                        return i;
                        //break;
                    }
                }
            }
            //           if (index == -1)
            //           {
            //Console.WriteLine($"Somehow, {player.AccountID} was not found in the server search of connections. Maybe we need to check on this?");
            //servers[0].logFile.WriteLine($"Somehow, {player.Guid} was not found in the server search of connections. Maybe we need to check on this?");
            //index = 0;
            //             return null;
            //       }
            return -1;// servers[index];
        }

        public static void StartNewTimer(Player player, int timerSeconds, string timerType, string whenTimerEnds)//base timer
        {
            //string 
            if (player.CurrentServer != null)
            {
                if (timerSeconds <= 0)
                {
                    //timer is 0 seconds or less, why are we even here?
                    if (timerType == "RT")
                    {
                        player.SendData("052", $"roundtime::0");
                        //SendData(player, "052", $"roundtime::0");
                    }
                    return;
                }

                ServerMain.PlayerTimers newTimer = new ServerMain.PlayerTimers();
                newTimer.player = player;
                newTimer.timerStart = DateTime.UtcNow;
                //SendData(player, newTimer.timerStart.ToString("HH:mm:ss:fff"));
                newTimer.timerLength = TimeSpan.FromSeconds(timerSeconds);
                newTimer.timerType = timerType;
                newTimer.atTimerEnd = whenTimerEnds;
                player.CurrentServer.playerTimers.Add(newTimer);

                switch (timerType)
                {
                    case "RT":
                        //send the RT message to the frontend
                        player.SendData("052", $"roundtime::{timerSeconds.ToString()}");
                        //SendData(player, "052", $"roundtime::{timerSeconds.ToString()}");
                        break;
                }
//                ServerMain.PlayerTimers newTimer = new ServerMain.PlayerTimers();
//                newTimer.player = player;
//                newTimer.timerStart = DateTime.UtcNow;
                //SendData(player, newTimer.timerStart.ToString("HH:mm:ss:fff"));
//                newTimer.timerLength = TimeSpan.FromSeconds(timerSeconds);
//                newTimer.timerType = timerType;
//                newTimer.atTimerEnd = whenTimerEnds;
//                player.CurrentServer.playerTimers.Add(newTimer);
  /*              if (timerType == "RT")
                {
                    if (timerSeconds > 0)
                    {
                        //if there is actual roundtime > 0, send the Roundtime: text
                        SendData(player, "052", $"roundtime::{timerSeconds.ToString()}");
                        //SendData(player, $"Roundtime: {timerSeconds} sec.");
                    } else
                    {
                        //if no actual roundtime, just send the rt message
                        SendData(player, "052", $"roundtime::{timerSeconds.ToString()}");
                    }
                }*/
                //SendData(player, ">");
            } else
            {
                Console.WriteLine($"Somehow, {player.AccountID} does not have a current server set. Maybe we need to check on this? ServerFunctions.StartNewTimer-base");
            }
        }

        public static bool IsPlayerInRT(Player player)
        {
            if (player.CurrentServer != null)
            {
                bool inRT = false;
                for (int i = 0; i < player.CurrentServer.playerTimers.Count; i++)
                {
                    if (player.CurrentServer.playerTimers[i].player == player && player.CurrentServer.playerTimers[i].timerType == "RT")
                    {
                        inRT = true;
                        break;
                    }
                }
                return inRT;
            } else
            {
                Console.WriteLine($"Somehow, {player.AccountID} does not have a current server set. Maybe we need to check on this? ServerFunctions.IsPlayerInRT");
                return false;
            }
        }

        public static ServerMain GetServer(Player player)
        {

            return null;
        }

        /// <summary>
        /// Returns true if player is still connected
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsClientStillConnected(Player player)
        {
            TaskStatus[] disconnection = new TaskStatus[]
            {
            TaskStatus.Canceled,
            TaskStatus.Faulted,
            TaskStatus.RanToCompletion
            };

            bool stillRunning = true;
            foreach (TaskStatus status in disconnection)
            {
                if (player.ClientTask.Status == status)
                {
                    stillRunning = false;
                    player.Guid = Guid.Empty;
                }
            }
            return stillRunning;
        }

        public static string GetBaseClassString(string classToGet)
        {
            string classString = "";

            switch (classToGet.ToLower())
            {
                case "room":
                    for (int i = 0; i < Config.baseClasses.Count; i++)
                    {
                        if (Path.GetFileNameWithoutExtension(Config.baseClasses[i]).EndsWith("Room"))
                        {
                            classString = File.ReadAllText(Config.baseClasses[i]);
                        }
                        //ServerConsole.newConsole.WriteLine(Config.baseClasses[i]);
                    }

                    break;


            }

            bool keepChecking = true;
            string[] removeUsings = { "using LiteDB;" };//don't worry about newtonsoft. we're using that already

            while (keepChecking)
            {//things to check if string contains, if do then remove them before sending the string to the client
                for (int i = 0; i < removeUsings.Length; i++)
                {
                    if (classString.IndexOf(removeUsings[i]) > -1)
                    {
                        classString = classString.Remove(classString.IndexOf(removeUsings[i]), removeUsings[i].Length);
                        continue;
                    }
                }

                if (classString.IndexOf("[BsonIgnore") > -1)
                {

                }
                if (classString.IndexOf("[JsonIgnore") > -1)
                {

                }

                keepChecking = false;
            }


            return classString;
        }

 /*       [Serializable]
        public class LoadedServer
        {
            public ServerMain serverClass;
            public Server serverInfo;
        }*/

    }

}
