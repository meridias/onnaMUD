using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using onnaMUD;
using onnaMUD.MUDServer;
using onnaMUD.Database;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
using onnaMUD.Settings;
using Microsoft.Extensions.Options;
using static onnaMUD.Utilities.ServerConsole;

namespace onnaMUD.Utilities
{
    public class ServerConsole
    {
        static public StreamWriter? newConsole;
        //static public bool checkQueue = false;
        //static private Task? commandTask;
        static public bool runConsole = false;

        public ServerConsole()
        {
            newConsole = new StreamWriter(Console.OpenStandardOutput());
            newConsole.AutoFlush = true;
        }

        public async Task ConsoleWindow()
        {
            runConsole = true;
            List<Option> options = new List<Option>();
            string menuWindow = "main";
            int serverIndex = 0;
            ServerMain workingServer = null;

            //after loading config, and check command line args, start any auto-start servers
            if (ServerFunctions.servers.Count > 0)
            {
                newConsole.WriteLine("Auto-starting servers...");
                await Task.Delay(1000);

                for (int i = 0; i < ServerFunctions.servers.Count; i++)//why does this cause the ConsoleWindow task to block CheckQueue?
                {
             //       if (ServerFunctions.servers[i].serverInfo.AutoStart)
             //       {
                        //newConsole.WriteLine($"Starting up {ServerFunctions.servers[i].serverInfo.Name} server...");//logFile Writelines now also get sent to console
             //           await ServerFunctions.servers[i].StartServer();
                        //newConsole.WriteLine($"Done.");
             //           await Task.Delay(1000);
             //       }
                }
                newConsole.WriteLine("Servers are done auto-starting.");
                await Task.Delay(1500);
            } else
            {
                newConsole.WriteLine("No servers are set for auto-start.");
                await Task.Delay(1000);
            }
            //newConsole.WriteLine($"{Environment.NewLine}");
            //Server workingServer = new Server();//the current server info we're dealing with, be it adding a new server or editing a current one

            //           if (ServerMain.servers.Count == 0)
            //           {
            //               newConsole.WriteLine("You currently somehow have no servers loaded from the appsettings.");
            //               newConsole.WriteLine("Kinda hard to interact with servers when there aren't any there, yes?");
            //               newConsole.WriteLine("Exiting...");
            //               await Task.Delay(2000);
            //               return;
            //           }
            //newConsole.WriteLine($"Welcome to the onnaMUD main console!{Environment.NewLine}-------------------------------");
            //checkQueue = true;
            //commandTask = CheckQueueForCommands();
            //ServerMain? tempServer = null;
            while (runConsole)// !IsServerRunning())
            {
                switch (menuWindow)
                {
                    case "main":
                        options.Clear();
                        newConsole.WriteLine($"{Environment.NewLine}Welcome to the onnaMUD main console!{Environment.NewLine}-------------------------------");
                        int numOfServers = ServerFunctions.servers.Count;

                        ShowServerStatus();
                        newConsole.WriteLine("");
                        SetOptions("main");
                        ShowOptions("main");

                        goto default;

                        //newConsole.Write(">");

                        //break;
           //         case "add":
          /*              options.Clear();
                        //Server newServer = new Server();
                        Server newServer = new Server();
                        newServer.Id = DB.DBAccess.GetNewGuid<Server>(DB.Collections.Server);
                        Room newRoom = new Room();
                        newRoom.Id = DB.DBAccess.GetNewGuid<Room>(DB.Collections.Room);
                        newRoom.RoomType = RoomType.CharCreate;
                        DB.DBAccess.Save(newRoom, DB.Collections.Room);
                        newServer.FirstRoom = newRoom.Id;
                        //DB.DBAccess.Save(newServer, DB.Collections.Server);
                        SaveServer(newServer);
                        //newConsole.WriteLine(temp.Id.ToString());
                        //newConsole.WriteLine(newRoom.Id.ToString());
                        //Server newServer = new Server();
                        //newServer.serverInfo = temp;
                        ServerMain temp = new ServerMain(newServer);
                        ServerFunctions.servers.Add(temp);
                        newConsole.WriteLine("New server added!");
                        await Task.Delay(1000);
                        menuWindow = "main";*/
            //            break;
                    case "edit":
                        options.Clear();
                        //this just gets the Server class from the database, which is referenced by serverInfo in ServerMain
                        //workingServer = GetServer(ServerFunctions.servers[serverIndex].serverID);//  DB.DBAccess.GetById<Server>(ServerFunctions.servers[serverIndex].serverID, DB.Collections.Server);
                        //workingServer.
                        SetOptions("edit");

        //                newConsole.WriteLine($"Editing {ServerFunctions.servers[serverIndex].serverInfo.Name} server.{Environment.NewLine}-------------------------------");
                        ShowOptions("edit");

                        goto default;

                    default:
                        //default is where we wait for input with # indexes, if we don't need this just break from switch as normal
                        newConsole.Write(">");
                        //use the switch to show the menu and get the options, be outside the switch to actually pick the options created
                        int menuOption;
                        //Int32.TryParse(Console.ReadLine(), out menuOption);

                        if (Int32.TryParse(Console.ReadLine(), out menuOption))// menuOption <= numOfServers)
                        {//parse was successful
                            if (menuOption <= options.Count && menuOption > 0)//  menuOption <= numOfServers && menuOption > 0)
                            {//option picked is in the range of options
                             //newConsole.WriteLine("picked!");

                                Option chosen = options[menuOption - 1];//  new Option();
                                /*   for (int i = 0; i < options.Count; i++)
                                   {
                                       if (options[i].index == menuOption)
                                       {
                                           chosen = options[i];
                                       }
                                   }*/
                                //newConsole.WriteLine(chosen.option);
                                switch (chosen.option)
                                {
                                    case "main":
                                        menuWindow = "main";
                                        break;
                                    case "add":
                                        //options.Clear();
                                        //Server newServer = new Server();
                                        Server newServer = new Server();
                                        newServer.Id = DB.DBAccess.GetNewGuid<Server>(DB.Collections.Server);
                                        Room newRoom = new Room();
                                        newRoom.Id = DB.DBAccess.GetNewGuid<Room>(DB.Collections.Room);
                                        newRoom.RoomType = RoomType.CharCreate;
                                        DB.DBAccess.Save(newRoom, DB.Collections.Room);
                                        newServer.FirstRoom = newRoom.Id;
                                        //DB.DBAccess.Save(newServer, DB.Collections.Server);
                                        //SaveServer(newServer);
                                        //newConsole.WriteLine(temp.Id.ToString());
                                        //newConsole.WriteLine(newRoom.Id.ToString());
                                        //Server newServer = new Server();
                                        //newServer.serverInfo = temp;
                                        ServerMain temp = new ServerMain();
                                        ServerFunctions.servers.Add(temp);
                                        SaveServer(temp);
                                        newConsole.WriteLine("New server added!");
                                        await Task.Delay(1000);
                                        //menuWindow = "main";//this should still be main since we didn't go to another 'window'
                                        //menuWindow = "add";
                                        break;
                                    case "edit":
                                        menuWindow = "edit";
                                        workingServer = chosen.serverToEdit;
                                        //serverIndex = chosen.server;
//                                        workingServer = DB.DBAccess.GetById<Server>(ServerFunctions.servers[serverIndex].serverID, DB.Collections.Server);
                                        break;
                                    case "editname":
                                        newConsole.WriteLine("Please enter the new server name.");
                                        newConsole.WriteLine("Note that this is NOT the title, just the internal name for the server");
                                        newConsole.Write(">");
                                        string newName = Console.ReadLine();
                                        //ServerFunctions.servers[serverIndex].serverName = newName;
                                        //Server workingServer = GetServer(ServerFunctions.servers[serverIndex].serverID);
                                        //Server tempServer = new Server();// workingServer;
                                        //Server tempServer = workingServer;
                                        //newConsole.WriteLine(ServerFunctions.servers[serverIndex].serverName);
                                        //Server editName = new Server();
                                        //tempServer.Id = ServerFunctions.servers[serverIndex].serverID;
                         //               workingServer.serverInfo.Name = newName;
                                        //newConsole.WriteLine(workingServer.Name);
                                        //editName.Name = newName;
                                        //editName.Debug = ServerFunctions.servers[serverIndex].debug;
                                        //editName.Type = ServerFunctions.servers[serverIndex].serverType;
                                        //ServerFunctions.servers[serverIndex].serverInfo.Name = newName;
                                        //Server tempServer = workingServer;
                                        SaveServer(workingServer);
                                        //DB.DBAccess.Save(tempServer, DB.Collections.Server);
                                        newConsole.WriteLine("Server name changed!");
                                        await Task.Delay(1000);
                                        break;
                                    /*              case "edittype":
                                                      menuWindow = "edittype";
                                                      newConsole.WriteLine("Please enter the server type, either login or game.");
                                                      newConsole.WriteLine("Note that you can only have 1 login server.");
                                                      newConsole.Write(">");
                                                      string newType = Console.ReadLine();
                                                      Server editType = new Server();
                                                      editType.Name = ServerFunctions.servers[serverIndex].serverName;
                                                      //editType.Type = newType;
                                                      editType.Debug = ServerFunctions.servers[serverIndex].debug;
                                                      switch (newType)
                                                      {
                                                          case "login":
                                                              //check any other servers to see if a login server is already set
                                                              for (int i = 0; i < ServerFunctions.servers.Count; i++)
                                                              {
                                                                  if (i != serverIndex && ServerFunctions.servers[i].serverType == "login")
                                                                  {
                                                                      //if the server type for another server is login AND we're not looking at the same server we're editing...
                                                                      newConsole.WriteLine("You already have another server set for login. If you would like to really edit this server to be the login, please change the other server to game first.");
                                                                      break;
                                                                  }
                                                              }
                                                              //if there was no other match?
                                                              ServerFunctions.servers[serverIndex].serverType = newType;
                                                              editType.Type = newType;
                                                              DB.DBAccess.Save<Server>(editType, DB.Collections.Server);
                                                              break;
                                                          case "game":
                                                              ServerFunctions.servers[serverIndex].serverType = newType;
                                                              editType.Type = newType;
                                                              DB.DBAccess.Save<Server>(editType, DB.Collections.Server);
                                                              break;
                                                          default:
                                                              //maybe a message, or just loop back?
                                                              break;


                                                      }
                                                      break;*/
                                    case "editport":
                                        newConsole.WriteLine("Enter the port number for this server to listen on. (1-65535)");
                                        newConsole.Write(">");
                                        string newPort = Console.ReadLine();
                                        if (VerifyIntRange(newPort, 1, 65535))
                                        {
                                            Int32.TryParse(newPort, out int port);
                             //               workingServer.serverInfo.ServerPort = port;
                                            SaveServer(workingServer);
                                            newConsole.WriteLine("Server listener port changed!");
                                        }
                                        await Task.Delay(1000);
                                        break;
                                    case "editautostart":
                                        newConsole.WriteLine("Does this server start automatically on executable start? (true or false)");
                                        newConsole.Write(">");
                                        string newAuto = Console.ReadLine();
                                        if (VerifyBool(newAuto))
                                        {
                           //                 workingServer.serverInfo.AutoStart = bool.Parse(newAuto);
                                            SaveServer(workingServer);
                                            newConsole.WriteLine("Server auto start status changed!");
                                            //await Task.Delay(1000);
                                        }
                                        await Task.Delay(1000);
                                        break;
                                    case "editdefault":
                                        //menuWindow = "editdefault";
                                        newConsole.WriteLine("Is this server available to all new accounts? (true or false)");
                                        newConsole.Write(">");
                                        string newDefault = Console.ReadLine();
                                        if (VerifyBool(newDefault))
                                        {
                      //                      workingServer.serverInfo.IsDefault = bool.Parse(newDefault);
                                            SaveServer(workingServer);
                                            newConsole.WriteLine("Server default access status changed!");
                                            //await Task.Delay(1000);
                                        }
                                        await Task.Delay(1000);
                                        /*       switch (newDefault.ToLower())
                                               {
                                                   case "true":
                                                       workingServer.serverInfo.IsDefault = true;
                                                       //DB.DBAccess.Save(workingServer, DB.Collections.Server);
                                                       SaveServer(workingServer);
                                                       newConsole.WriteLine("Server default access status changed!");
                                                       await Task.Delay(1000);
                                                       break;
                                                   case "false":
                                                       workingServer.serverInfo.IsDefault = false;
                                                       //DB.DBAccess.Save(workingServer, DB.Collections.Server);
                                                       SaveServer(workingServer);
                                                       newConsole.WriteLine("Server default access status changed!");
                                                       await Task.Delay(1000);
                                                       break;
                                                   default:
                                                       newConsole.WriteLine("Please enter only true or false");
                                                       break;
                                               }*/
                                        break;
                                    case "editdebug":
                                        //menuWindow = "editdebug";
                                        newConsole.WriteLine("Does this server start in debug mode? (true or false)");
                                        newConsole.Write(">");
                                        string newDebug = Console.ReadLine();
                                        if (VerifyBool(newDebug))
                                        {
                                            //if the input was verified as being true or false
                     //                       workingServer.serverInfo.Debug = bool.Parse(newDebug);
                                            SaveServer(workingServer);
                                            newConsole.WriteLine("Server debug status changed!");
                                            //await Task.Delay(1000);
                                        }
                                        await Task.Delay(1000);
                                        /*         switch (newDebug.ToLower())
                                                 {
                                                     case "true":
                                                         workingServer.serverInfo.Debug = true;
                                                         //DB.DBAccess.Save(workingServer, DB.Collections.Server);
                                                         SaveServer(workingServer);
                                                         newConsole.WriteLine("Server debug status changed!");
                                                         await Task.Delay(1000);
                                                         //menuWindow = "edit";
                                                         break;
                                                     case "false":
                                                         workingServer.serverInfo.Debug = false;
                                                         //DB.DBAccess.Save(workingServer, DB.Collections.Server);
                                                         SaveServer(workingServer);
                                                         newConsole.WriteLine("Server debug status changed!");
                                                         await Task.Delay(1000);
                                                         //menuWindow = "edit";
                                                         break;
                                                     default:
                                                         newConsole.WriteLine("Please enter only true or false");
                                                         //menuWindow = "edit";
                                                         break;
                                                 }*/
                                        break;
                                    case "start":
                                        newConsole.WriteLine($"Starting up {Config.config.gameName} server...");
                                        chosen.serverToEdit.StartServer();
                                        //ServerFunctions.servers[serverIndex].StartServer();
                                        newConsole.WriteLine($"Done.");
                                        await Task.Delay(1000);
                                        //menuWindow = "start";
                                        //serverIndex = chosen.server;
                                        break;
                                    case "stop":
                                        newConsole.WriteLine($"Shutting down {Config.config.gameName} server...");
                                        chosen.serverToEdit.StopServer();
                                        //ServerFunctions.servers[serverIndex].StopServer();
                                    //    if (ServerFunctions.servers[serverIndex].serverType == "game")//since login server isn't using loaded assemblies for commands
                                      //  {
                                            newConsole.Write("Unloading command assemblies");
                                            while (chosen.serverToEdit.assemWeak.IsAlive)
                                            {
                                                GC.Collect();
                                                GC.WaitForPendingFinalizers();
                                                newConsole.Write(".");
                                                await Task.Delay(1000);
                                            }
                                     //   }
                                        newConsole.WriteLine($"{Environment.NewLine}Done.");
                                        await Task.Delay(1000);
                                      //  menuWindow = "stop";
                                      //  serverIndex = chosen.server;
                                        break;
                                }
                            }
                            else
                            {//option picked is not in the range of options
                                newConsole.WriteLine("Invalid selection! Please enter the number for the option you want.");
                                await Task.Delay(1500);
                            }
                        }
                        else
                        {//parse failed
                            newConsole.WriteLine("Invalid selection! Please enter the number for the option you want.");
                            await Task.Delay(1500);
                        }
                        break;

                }

                //Console.WriteLine(Environment.NewLine);
                //Console.ReadLine();
            }

            void ShowServerStatus()
            {
                if (ServerFunctions.servers.Count == 0)
                {
                    newConsole.WriteLine("No servers found in the database. Please create least 1 game server to get things going.");
                }
                else
                {
                    //show servers current status: up (with listener port) or not started
                    for (int i = 0; i < ServerFunctions.servers.Count; i++)// ServerFunctions.LoadedServer server in ServerFunctions.servers)
                    {
       //                 newConsole.Write($"Server \"{ServerFunctions.servers[i].serverInfo.Name}\" status: ");
                        if (ServerFunctions.servers[i].serverIsRunning)
                        {
         //                   newConsole.WriteLine($"Up: listening on port {ServerFunctions.servers[i].serverInfo.ServerPort}");
                        }
                        else
                        {
                            newConsole.WriteLine("Not started");
                        }
                    }
                }
            }

            void ShowOptions(string optionsToShow)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    switch (options[i].option)
                    {
                        case "add":
                            newConsole.WriteLine($"{i + 1}) Add server");
                            break;
                        case "start":
      //                      newConsole.WriteLine($"{i + 1}) Start {options[i].serverToEdit.serverInfo.Name} server");
                            break;
                        case "stop":
     //                       newConsole.WriteLine($"{i + 1}) Shutdown {options[i].serverToEdit.serverInfo.Name} server");
                            break;
                        case "edit":
     //                       newConsole.WriteLine($"{i + 1}) Edit {options[i].serverToEdit.serverInfo.Name} server");
                            break;
                        case "main":
                            newConsole.WriteLine("");
                            newConsole.WriteLine($"{i+1}) Back to main menu");
                            break;
                        case "editname":
                            newConsole.WriteLine($"{i+1}) Set server name");
                            break;
                        case "editdebug":
                            newConsole.WriteLine($"{i+1}) Set if server starts in debug mode (true or false)");
         //                   newConsole.WriteLine($"    Currently is: {workingServer.serverInfo.Debug}");
                            break;
                        case "editdefault":
                            newConsole.WriteLine($"{i + 1}) Set if this server is available to all new accounts (true or false)");
         //                   newConsole.WriteLine($"    Currently is: {workingServer.serverInfo.IsDefault}");
                            break;
                        case "editautostart":
                            newConsole.WriteLine($"{i + 1}) Set if this server is automatically started (true or false)");
         //                   newConsole.WriteLine($"    Currently is: {workingServer.serverInfo.AutoStart}");
                            break;
                        case "editport":
                            newConsole.WriteLine($"{i + 1}) Set listener port for this server (1-65535)");
         //                   newConsole.WriteLine($"    Currently is: {workingServer.serverInfo.ServerPort}");
                            break;

                    }
                }

                /*      switch (optionsToShow)
                      {
                          case "main":
                              if (ServerFunctions.servers.Count == 0)
                              {
                                  newConsole.WriteLine($"1) Add server");
                                  Option addServer = new Option();
                                  addServer.index = 1;
                                  addServer.option = "add";
                                  options.Add(addServer);
                              }
                              else
                              {
                                  for (int i = 0; i < ServerFunctions.servers.Count; i++)// ServerFunctions.LoadedServer server in ServerFunctions.servers)
                                  {
                                      Option option = new Option();
                                      if (i == 0)
                                      {
                                          option.index = 1;
                                      }
                                      else
                                      {
                                          option.index = options.Last().index + 1;
                                      }
                                      //option.index = i + 1;
                                      if (ServerFunctions.servers[i].serverIsRunning)
                                      {
                                          newConsole.WriteLine($"{option.index}) Shutdown {ServerFunctions.servers[i].serverName} server");
                                          option.server = i;
                                          option.option = "stop";
                                          options.Add(option);
                                      }
                                      else
                                      {
                                          newConsole.WriteLine($"{option.index}) Start {ServerFunctions.servers[i].serverName} server");
                                          option.server = i;
                                          option.option = "start";
                                          options.Add(option);
                                      }
                                  }
                                  newConsole.WriteLine("");
                                  for (int i = 0; i < ServerFunctions.servers.Count; i++)
                                  {
                                      Option option = new Option();
                                      option.index = options.Last().index + 1;//  i + ServerFunctions.servers.Count + 1;
                                      newConsole.WriteLine($"{option.index}) Edit {ServerFunctions.servers[i].serverName} server");
                                      option.option = "edit";
                                      option.server = i;
                                      options.Add(option);
                                  }

                                  //newConsole.WriteLine($"{numOfServers + 1}) Add server");
                                  Option addServer = new Option();
                                  addServer.index = options.Last().index + 1;
                                  newConsole.WriteLine($"{addServer.index}) Add server");
                                  addServer.option = "add";
                                  options.Add(addServer);
                              }
                              break;
                      }*/
            }

            bool VerifyBool(string inputString)//true if valid bool, false if not valid bool (whether it's true or false)
            {
                try
                {
                    bool temp = bool.Parse(inputString);
                    return true;
                }
                catch (Exception ex)
                {
                    newConsole.WriteLine("Please enter only true or false.");
                    return false;
                }
            }

            bool VerifyIntRange(string inputString, int minRange, int maxRange)//true if input is int within range, false if not
            {
                if (Int32.TryParse(inputString, out int port))
                {
                    if (port >= minRange && port <= maxRange)
                    {
                        //within range
                        return true;
                    } else
                    {
                        //outside range
                        newConsole.WriteLine("Please enter a number that is within the specified range.");
                        return false;
                    }
                }
                else
                {
                    newConsole.WriteLine("Invalid selection! Please enter a valid number.");
                    //await Task.Delay(2000);
                    return false;
                }
            }

            void SetOptions(string optionsToSet)
            {
                options.Clear();

                switch (optionsToSet)
                {
                    case "main":
                        if (ServerFunctions.servers.Count == 0)
                        {
                            Option addServer = new Option();
                            //addServer.index = 1;
                            addServer.option = "add";
                            options.Add(addServer);
                        }
                        else
                        {
                            for (int i = 0; i < ServerFunctions.servers.Count; i++)//add the start/stop options for current servers
                            {
                                Option option = new Option();
                                if (ServerFunctions.servers[i].serverIsRunning)
                                {
                                    //   newConsole.WriteLine($"{option.index}) Shutdown {ServerFunctions.servers[i].serverName} server");
                                    //option.server = i;
                                    option.serverToEdit = ServerFunctions.servers[i];
                                    option.option = "stop";
                                    options.Add(option);
                                }
                                else
                                {
                                    //    newConsole.WriteLine($"{option.index}) Start {ServerFunctions.servers[i].serverName} server");
                                    //option.server = i;
                                    option.serverToEdit = ServerFunctions.servers[i];
                                    option.option = "start";
                                    options.Add(option);
                                }
                            }
                            for (int i = 0; i < ServerFunctions.servers.Count; i++)//add the edit options for all current servers
                            {
                                Option option = new Option();
                                //option.index = options.Last().index + 1;//  i + ServerFunctions.servers.Count + 1;
                                //newConsole.WriteLine($"{option.index}) Edit {ServerFunctions.servers[i].serverName} server");
                                option.option = "edit";
                                option.serverToEdit = ServerFunctions.servers[i];
                                //option.server = i;
                                options.Add(option);
                            }
                            //add the add option for a new server
                            Option addServer = new Option();
                            //addServer.index = options.Last().index + 1;
                            //newConsole.WriteLine($"{addServer.index}) Add server");
                            addServer.option = "add";
                            options.Add(addServer);
                        }
                        break;
                    case "edit":
                        for (int i = 0; i < 6; i++)
                        {
                            Option option = new Option();
                            switch (i)
                            {
                                case 0:
                                    option.option = "editname";
                                    //option.index = 1;
                                    break;
                                case 1:
                                    option.option = "editdebug";
                                    //option.option = "edittype";
                                    //option.index = 2;
                                    break;
                                case 2:
                                    option.option = "editdefault";
                                    //option.option = "editdebug";
                                    //option.index = 3;
                                    break;
                                case 3:
                                    option.option = "editautostart";
                                    //option.option = "editdefault";
                                    //option.index = 4;
                                    break;
                                case 4:
                                    option.option = "editport";
                                    //option.option = "main";
                                    //option.index = 5;
                                    break;
                                case 5:
                                    option.option = "main";
                                    //option.index = 6;
                                    break;
                            }
                            options.Add(option);
                        }
                        break;

                }
            }

        }

 /*       public static void SendConsole(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            if (Program.pipeStream != null)
            {
                Program.pipeStream.Write(msg);
                Program.pipeStream.Flush();
            }



        }*/


        public Server GetServer(Guid serverGuid)
        {
            var server = DB.DBAccess.GetById<Server>(serverGuid, DB.Collections.Server);
            return server;
        }

        public void SaveServer(ServerMain server)
        {
            //update Server class in database
   //         DB.DBAccess.Save(server.serverInfo, DB.Collections.Server);
            //update serverMain with updated ServerInfo
            for (int i = 0; i < ServerFunctions.servers.Count; i++)
            {
                if (ServerFunctions.servers[i] == server)
                {
   //                 ServerFunctions.servers[i].serverInfo = server.serverInfo;
                    //newConsole.WriteLine("saved!");
                }
            }
        }

        public async Task StartOrStopServer(ServerMain? tempServer)
        {

            if (tempServer == null)
                return;

            if (tempServer.serverIsRunning)
            {//is running, shut down
    //            newConsole.WriteLine($"Shutting down {tempServer.serverInfo.Name} server...");
                tempServer.StopServer();
                newConsole.Write("Unloading command assemblies");
                GC.Collect();
                while (tempServer.assemWeak.IsAlive)
                {
                    //GC.Collect();
                    GC.WaitForPendingFinalizers();
                    newConsole.Write(".");
                    await Task.Delay(1000);
                }
                newConsole.WriteLine($"{Environment.NewLine}Done.");
                await Task.Delay(1000);
            }
            else
            {//is not running, start up
   //             newConsole.WriteLine($"Starting up {tempServer.serverInfo.Name} server...");
                tempServer.StartServer();
                newConsole.WriteLine($"Done.");
                await Task.Delay(1000);
            }

        }

        public class Option
        {
            public string option = "";
            public int index = 0;
            public int server = 0;
            public ServerMain serverToEdit;
        }

    }
}
