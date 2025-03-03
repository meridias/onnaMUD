using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Loader;
//using static onnaMUD.ServerMain;
using System.Data.SQLite;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;
using System.Data;
using onnaMUD.Settings;
using onnaMUD.Database;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
using onnaMUD.Characters;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Numerics;
using System.IO;
using onnaMUD.Temps;
using System.Xml.Linq;
using System.IO.Pipes;

namespace onnaMUD.MUDServer
{
    public class ServerMain
    {
        //public Config.SettingsServers? currentServer;
        //public string serverName;
        //public string serverType;
        //public bool debug;
        //public bool isDefault;
        //public Guid firstRoom;
        //public Guid serverID;
        //public int port;
        //public bool autoStart;
        //public Server serverInfo;

        public string serverSubDir = "";

        public int numOfBytes = 1024;
        //static public ServerInfo? currentServer = null;
        //private IPAddress ipAddress;//the ip and port that this server is listening on
        //private int port;
        //private int serverPort;
        //private string? serverType;
        //private string? serverName;
        private TcpListener? listener;
        private Task? listenerTask;
        private Task? logoutTask;
        private Task? roundtimeCheck;
        private Task? randomOutput;
        //private TcpListener? serverListener;//the listener on the login server for the game servers to connect to
        //private TcpClient? loginClient;
        private bool isListening = false;
        private bool doLogouts = false;
        private bool checkRTs = false;
        private bool doRandom = false;
        //private List<Task> listenerTasks = new List<Task>();
        //private Task? listenerTask;
        //private Task? serverListenerTask;
        //private List<Task> clientTasks = new List<Task>();//this is for all incoming data tasks for all clients (server connections and users) so we can check them for dropped connections
        //private List<Task> serverConnections = new List<Task>();
        //private List<TcpClient> connectedServers = new List<TcpClient>();//connected game servers on the login server
        //private IPEndPoint endPoint;
        //public IPHostEntry host = Dns.GetHostEntry("localhost");
        //public IPAddress ipAddress = host.AddressList[0];
        //public IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
        //public ManualResetEvent serverResetEvent = new ManualResetEvent(false);
        public bool serverIsRunning = false;

        //private Guid serverGuid = Guid.Empty;

        public List<Connections> connections = new List<Connections>();//this is for connections that have been authenticated (valid accounts, not banned, etc) that have
                                                                       //been passed off to a game server. keep that connection in the list until a game server wants verification on client connection, send approval if that client is in this
                                                                       //list, then clear it from the list
                                                                       //private SQLiteConnection conn;// = new SQLiteConnection(currentServer.sqliteConn);
        public List<Character> characters = new List<Character>();//all active characters (PC, NPC and mobs)
        public StreamWriter logFile;
        private List<Commands> adminCommands = new List<Commands>();//we'll set these at server startup for all commands that are part of the server core, not a dll
        private List<Commands> commandList = new List<Commands>();
        //public List<NewTrial> newTrialAccounts = new List<NewTrial>();
        public List<CommandQueue> commandQueue = new List<CommandQueue>();
        public List<InstancedRegions> regions = new List<InstancedRegions>();
        public AssemblyLoadContext? commandAssembly;// = new AssemblyLoadContext("commands", true);
        public WeakReference assemWeak;

        public List<PlayerTimers> playerTimers = new List<PlayerTimers>();
        public List<Task> tasks = new List<Task>();

        //cache stuff here to make it easier on the database?
        public List<Room> roomCache = new List<Room>();
        public NamedPipeServerStream? namedPipeServer = null;// new NamedPipeServerStream(Config.pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);// new NamedPipeServerStream(Config.pipeName, PipeDirection.InOut, 1);//, PipeTransmissionMode.Message);Message only works for windows?
        public Task? serverPipeRead = null;
        private bool readFromPipe = false;
        public TaskCompletionSource<bool> isOutputConnected = new TaskCompletionSource<bool>(false);

        //hopefully I'll eventually get to the point where the server class itself handles just the scheduling of tasks and other management/maintenance stuff
        //the ServerFunctions will have all the actual functions to do stuff
        public ServerMain()// string name, string type, bool doDebug)//  string serverToStart)// ServerMain.ServerInfo serverToStart)
        {
            //create the NamedPipeServerStream here before anything else so we can get the pipe connected to the temp output/server console
            //for the server startup messages
            //then load the config for the server
            //do all the database checks, account info, etc in the StartServer method

            //          configLoaded = Config.LoadConfig();//true if we loaded an existing appSettings, false if we had to make a new one
            try
            {
                namedPipeServer = new NamedPipeServerStream(Config.pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                tasks.Add(WaitForPipeClient());
            }
            catch (IOException)
            {
                //tried to start another server instance

            }

            //  Config.SettingsServers? currentServer = Config.GetServerInfo(serverToStart);

            //  if (currentServer == null)
            //  {
            //      Console.WriteLine($"Settings for server {serverToStart} could not be found or parsed correctly.");
            //      Console.WriteLine("Please check appsettings.json for correct information. Exiting...");
            //      return;
            //  } else
            //  {
            //serverInfo = server;
            //serverName = server.Name;
            //serverType = server.Type;
            //debug = server.Debug;
            //isDefault = server.IsDefault;
            //firstRoom = server.FirstRoom;
            //serverID = server.Id;
            //port = server.ServerPort;
            //autoStart = server.AutoStart;
            //  }
        }

        /*           public Server(int serverListIndex)
                   {
                       ServerInfo serverInfo = serversList[serverListIndex];
                       ipAddress = serverInfo.ipAddress;
                       port = serverInfo.port;
                       serverType = serverInfo.type;
                       serverName = serverInfo.serverName;

                       if (serverInfo.type == "login")
                       {
                           serverPort = serverInfo.serverPort;
                       }
                       Console.Title = $"{Config.ConfigInfo("gameName")} {serverInfo.serverName.ToUpper()} server";
                       //Console.WriteLine("just checking");

                       //Process.GetCurrentProcess().EnableRaisingEvents = true;
                       //Process.GetCurrentProcess().Exited += ServerHasShutdown;// new EventHandler(ServerHasShutdown);

                       StartServer();

                       //Console.WriteLine("blah");
                       //Console.ReadLine();
                       while (true)//while true? maybe some other bool might be better here
                       {
                           //check tasks are 
                           if (serverHasStarted)
                           {
                               if (clientTasks.Count > 0)
                               {
                                   for (int i = 0; i < clientTasks.Count; i++)
                                   {
                                       if (clientTasks[i].Status != TaskStatus.Running)//if the status of this task is not running (do we need to check for anything else? if it's anything else besides running, something's wrong?)
                                       {
                                           //TcpClient tempClient = clientTasks[i].
                                       } else
                                       {
                                           //Console.WriteLine("doh");
                                       }
                                   }
                               } else
                               {
                                   //Console.WriteLine("blahblah");
                               }
                               //Console.WriteLine("waiting");
                               Thread.Sleep(1000);//pause for 1 second every loop
                           } else
                           {
                               //Thread.Sleep(1000);//wait for a second
                           }

                           //Console.WriteLine("blah");
                       }
                       //Console.ReadLine();

                   }*/


        public async Task<Task> StartServer()//  public void StartServer()
        {
            if (!Config.LoadConfig())
                return null;//if we had to create a new appsettings.json file, exit so user can edit file as needed

            serverSubDir = Directory.GetCurrentDirectory();//  Path.Combine(Directory.GetCurrentDirectory(), Config.config.gameName);

            //make sure the main server directory and logs directory are created
     //       if (!Directory.Exists(serverSubDir))
     //       {
                //main server subdirectory
     //           Directory.CreateDirectory(serverSubDir);
     //       }
            if (!Directory.Exists(Path.Combine(serverSubDir, "logs")))
            {
                //subdirectory for log files
                Directory.CreateDirectory(Path.Combine(serverSubDir, "logs"));
            }
            if (!Directory.Exists(Path.Combine(serverSubDir, "plugins")))
            {
                //subdirectory for dll plugin files (commands, etc.)
                Directory.CreateDirectory(Path.Combine(serverSubDir, "plugins"));
            }

            /*           if (!Directory.Exists(Path.Combine(serverSubDir, "commands")))
                       {
                           //subdirectory for compiled command dll files
                           Directory.CreateDirectory(Path.Combine(serverSubDir, "commands"));
                       }
                       string scriptDir = Path.Combine(Directory.GetCurrentDirectory(), "command scripts");
                       if (serverType == "game")// serverType == "game")
                       {
                           //if this is a game server
                           if (!Directory.Exists(Path.Combine(scriptDir, serverName)))
                           {
                               //subdirectory under 'command scripts' for this server to put command .cs scripts to be compiled from
                               Directory.CreateDirectory(Path.Combine(scriptDir, serverName));
                           }
                       }*/

            logFile = new TimeStampedStream(Config.GetNextLogFileName(Path.Combine(serverSubDir, "logs"), Config.config.gameName));
            logFile.AutoFlush = true;

            logFile.WriteLine($"Starting up {Config.config.gameName} server...");
            //ServerConsole.newConsole.WriteLine($"Starting up {Config.config.gameName} server...");
            ConsoleOutput.SendConsole($"Starting up {Config.config.gameName} server...");
            await Task.Delay(1000);
            //these are now part of the normal commands in CommandsTest, or whatever group of commands I'll have
            /*           //manually add trial command here for all servers
                       Trial trial = new Trial();
                       for (int i = 0; i < trial.Aliases.Length; i++)
                       {
                           Commands tempCom = new Commands();
                           tempCom.commandAlias = trial.Aliases[i];
                           tempCom.commandClass = trial;
                           commandList.Add(tempCom);
                       }
                       //manually add choose command here for all servers
                       //gonna take this out of login servers in favor of clickable links only
                       //once I get it all set the way I want
                       Choose choose = new Choose();
                       for (int i = 0; i < choose.Aliases.Length; i++)
                       {
                           Commands tempCom = new Commands();
                           tempCom.commandAlias = choose.Aliases[i];
                           tempCom.commandClass = choose;
                           commandList.Add(tempCom);
                       }*/

            //do stuff that every server does
            logoutTask = CharacterLogoutTask();//this is what removes logged-in characters from the list after the client has/has been disconnected

            //          switch (serverType)//this is for login/game server type specific stuff, ie: game servers will have other tasks that need to be run: time-keeping, npcs, etc
            //          {
            //               case "login":

            //                    commandAssembly = new AssemblyLoadContext("commands", true);//do we even need these just for the login server?
            //                    assemWeak = new WeakReference(commandAssembly);
            //Commands tempCom = new Commands();
            //tempCom.commandName = "trial";
            //commandList.Add("trial");
            //commandList.Add(tempCom);
            //serverGuid = Guid.NewGuid();//do we even need this?
            //listenerTask = StartListening();
            //logFile.WriteLine("logoutTask");
            //logoutTask = CharacterLogoutTask();//this is what removes logged-in characters from the list after the client has/has been disconnected
            //logFile.WriteLine("remove dll");
            //                    RemoveCompiledDLLs(Path.Combine(serverSubDir, "commands"));
            //logFile.WriteLine("compile");
            //                    CompileCommands("all", Path.Combine(serverSubDir, "commands"));
            //CompileCommands(Path.Combine(Config.scriptDir, "all"), Path.Combine(serverSubDir, "commands"));
            //CompileCommands(Path.Combine(serverSubDir, "scripts"), Path.Combine(serverSubDir, "commands"));
            //logFile.WriteLine("load");
            //                    LoadCommandDLLs(Path.Combine(serverSubDir, "commands"));
            //serverListener = new TcpListener(ipAddress, serverPort);
            //serverListener = TcpListener.Create(currentServer.serverPort);
            //serverListener.Start();
            //listenerTasks.Add(StartListeningForGameServers());
            //serverListenerTask = StartListeningForGameServers();
            //start login gameserverlistener task
            //                    logFile.WriteLine($"{loadedCommands} command dlls loaded into command list!");
            //                   logFile.WriteLine($"{commandList.Count} total command aliases in list!");
            //                    ServerConsole.newConsole.WriteLine($"{loadedCommands} command dlls loaded into command list!");
            //                    ServerConsole.newConsole.WriteLine($"{commandList.Count} total command aliases in list!");
            //                    listenerTask = StartListening();//now we're just gonna have each server listen on its own port
            //                    break;
            //               case "game"://we'll need to figure out live/test specific stuff eventually
            logFile.WriteLine("Loading admin commands");//eventually
            //ServerConsole.newConsole.WriteLine("Loading admin commands");
            ConsoleOutput.SendConsole("Loading admin commands");
            await Task.Delay(1000);

            //blah, load commands
            logFile.WriteLine("Admin commands loaded");//eventually
            //ServerConsole.newConsole.WriteLine("Admin commands loaded");
            ConsoleOutput.SendConsole("Admin commands loaded");
            commandAssembly = new AssemblyLoadContext("commands", true);
            assemWeak = new WeakReference(commandAssembly);
            //empty out compiled command directory
            //compile and create dll for all command scripts in command scripts directory
            //load all compiled dlls for commands
            //                    RemoveCompiledDLLs(Path.Combine(serverSubDir, "commands"));
            //                    CompileCommands(serverName, Path.Combine(serverSubDir, "commands"));
            //CompileCommands(Path.Combine(serverSubDir, "scripts"), Path.Combine(serverSubDir, "commands"));
            //LoadCommandDLL(Path.Combine(serverSubDir, "commands"));
            LoadCommandDLL(serverSubDir);
            //ServerConsole.newConsole.WriteLine("Caching rooms from database...");
            logFile.WriteLine("Caching rooms from database...");
            //ServerConsole.newConsole.WriteLine("Caching rooms from database...");
            ConsoleOutput.SendConsole("Caching rooms from database...");
            await Task.Delay(1000);
            roomCache = DB.DBAccess.GetList<Room>(DB.Collections.Room);
            //ServerConsole.newConsole.WriteLine("Rooms loaded!");
            logFile.WriteLine("Done loading rooms");
            //ServerConsole.newConsole.WriteLine("Done loading rooms");
            ConsoleOutput.SendConsole("Done loading rooms");
            await Task.Delay(1000);
            roundtimeCheck = CheckPlayerTimers();
            //randomOutput = RandomOutput();
            listenerTask = StartListening();
            logFile.WriteLine($"{Config.config.gameName} server started!");
            //ServerConsole.newConsole.WriteLine($"{Config.config.gameName} server started!");
            ConsoleOutput.SendConsole($"{Config.config.gameName} server started!");
            //                  break;

            //           }

            //logFile.WriteLine($"{serverName} server started!");
            //serverPipeRead = ReadFromPipe();
            //tasks.Add(WaitForPipeClient());
            //tasks.Add(ReadFromPipe());

            //start a task to keep the server running and return that task
            serverIsRunning = true;

            
            return ServerRunning();
        }

        async Task WaitForPipeClient()
        {
            try
            {
                await namedPipeServer.WaitForConnectionAsync();
                tasks.Add(ReadFromPipe());
                isOutputConnected.SetResult(true);
            }
            catch (Exception ex)
            {
                logFile.WriteLine($"WaitForPipeClient: {ex}");
                return;
            }

        }

        async Task ReadFromPipe()
        {
            StreamReader pipeReading = new StreamReader(namedPipeServer);
            int numOfChar = 1024;
            char[] chars = new char[numOfChar];
            string receivedStringBuffer = "";
            bool checkMessage = false;
            readFromPipe = true;

            while (readFromPipe)
            {
                try
                {
                    int charsRec = await pipeReading.ReadAsync(chars, 0, numOfChar);
                    if (charsRec > 0)
                    {
                        receivedStringBuffer += new string(chars);
                        checkMessage = true;
                    }
                    else
                    {
                        readFromPipe = false;
                        //return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    readFromPipe = false;
                }

                while (checkMessage)
                {
                    Console.WriteLine(receivedStringBuffer);
                    //                  if (blah)
                    //                  {



                    //                  } else
                    //                  {
                    checkMessage = false;
                    //                  }
                }
            }
        }

        public async Task ServerRunning()
        {
            await Task.WhenAll(tasks);
         //   while (serverIsRunning)
         //   {



         //       await Task.Delay(1);
         //   }
        }

        public async Task StartListening()
        {
            listener = TcpListener.Create(Config.config.port);
            listener.Start();
            isListening = true;
            //Console.WriteLine()
            logFile.WriteLine($"{Config.config.gameName} {Config.config.gameName} listening on port {Config.config.port}");
            ServerConsole.newConsole.WriteLine($"{Config.config.gameName} {Config.config.gameName} listening on port {Config.config.port}");
            while (isListening)
            {
                try
                {
                    //TcpClient incomingClient = await listener.AcceptTcpClientAsync();
                    //Console.WriteLine("waiting?");

                    //check to see if we're on the login server or game servers?
                    //might need to add tcpclient to different lists, depending.
                    TcpClient incomingClient = await listener.AcceptTcpClientAsync();

                    //we need this here so the DoLogin has the player to interact with
                    Player newConnection = new Player();

                    //newConnection.Roundtime = new Timer(newConnection.EndRoundtime, newConnection, -1, -1);
                    //check if guid is used already?
                    //newConnection.Guid = Guid.NewGuid();
                    newConnection.Client = incomingClient;
                    newConnection.CurrentServer = this;
                    //  newConnection.IP = incomingClient.Client.RemoteEndPoint.ToString();
                    //accountID and accountType are set on the player at successful login down in processMessage
                    //clientTask itself?

                    ServerFunctions.SendData(newConnection, "052", $"gameName::{Config.config.gameName}");
                    //ServerFunctions.SendData(newConnection, "doh!");

                    //don't wait for this to finish so we're not holding up the line for the next person for however long it takes for this person to get logged in
                    ServerFunctions.DoLogin(newConnection);

                    //Connections tempConn = new Connections();
                    //tempConn.guid = Guid.NewGuid();//not sure I should put this here, was thinking of just having the guid on logged in characters, not accounts
                    //tempConn.client = incomingClient;
                    //tempConn.clientIP = incomingClient.Client.RemoteEndPoint.ToString();
                    //tempConn.serverOrUser = "user";
                    //Guid guid = Guid.NewGuid();
                    //               tempConn.player = newConnection;
                    //               tempConn.clientTask = ConnectedClient(newConnection);// incomingClient, newConnection);// tempConn.guid);
                    //               connections.Add(tempConn);
                    //               ServerFunctions.SendData(newConnection, "052", newConnection.Guid.ToString());
                    //SendData(tempConn.guid, "052", tempConn.guid.ToString());
                    //clientTasks.Add(ConnectedClient(incomingClient));
                    //clientTasks.Add(ConnectedClient(await listener.AcceptTcpClientAsync()));//keep this list here, so if we need to shut down we can clear all the
                    //connections that haven't been kicked or passed off to a game server yet
                }
                catch (SocketException)// se)
                {
                    logFile.WriteLine("We have shut down the listener");
                    isListening = false;
                }
                catch (Exception ex)
                {
                    logFile.WriteLine($"Blah!? {ex.Message}");
                    isListening = false;
                    //listener.Stop();
                }
            }

        }

        public async Task RandomOutput()
        {
            Random rnd = new Random();
            doRandom = true;
            while (doRandom)
            {
                await Task.Delay(rnd.Next(10, 41) * 1000);//random interval between 1000 and 30000 milliseconds (1-30 seconds)
                for (int i = 0; i < connections.Count; i++)
                {
                    //all the people in the connections on this server, if they're in an actual room
                    if (connections[i].player.character.CurrentRoom != Guid.Empty)
                    {
                        ServerFunctions.SendData(connections[i].player, "<br>Random person walking by...");

                    }

                }

            }
        }

        public async Task CheckPlayerTimers()
        {
            checkRTs = true;

            /*          while (checkRTs)
                      {
                          for (int i = 0; i < connections.Count; i++)
                          {
                              if (connections[i].player.RTWatch.IsRunning && connections[i].player.RTWatch.ElapsedMilliseconds >= connections[i].player.Roundtime)
                              {
                                  //ServerFunctions.SendData(connections[i].player, "roundtime is done");
                                  //if the stopwatch is running and the stopwatch is greater or equal to roundtime, then we've finished our roundtime
                                  connections[i].player.RTWatch.Stop();
                                  ServerFunctions.SendData(connections[i].player, "052", "ready");
                                  connections[i].player.RTWatch.Reset();
                              } else
                              {
                                  //ServerConsole.newConsole.WriteLine("nobody in RT");
                              }
                          }
                          await Task.Delay(1);
                      }*/

            //redo how timers are run/checked?
            //if (DateTime.UtcNow - startTime > breakDuration)
            while (checkRTs)
            {
                for (int i = 0; i < playerTimers.Count; i++)
                {
                    if (DateTime.UtcNow - playerTimers[i].timerStart > playerTimers[i].timerLength)
                    {
                        switch (playerTimers[i].atTimerEnd)
                        {
                            case "endRT":
                                //ServerFunctions.SendData(playerTimers[i].player, "052", "ready");//nope
                                //ServerFunctions.SendData(playerTimers[i].player, ">");
                                //ServerFunctions.SendData(playerTimers[i].player, DateTime.UtcNow.ToString("HH:mm:ss:fff"));
                                playerTimers.RemoveAt(i);
                                break;


                        }

                    }

                }

                await Task.Delay(1);
            }

        }

        public void RemoveCompiledDLLs(string commandDirectory)
        {
            String[] commandDLLs = Directory.GetFiles(commandDirectory, "*.dll", SearchOption.TopDirectoryOnly);

            if (commandDLLs.Length > 0)
            {
                for (int i = 0; i < commandDLLs.Length; i++)
                {
                    File.Delete(commandDLLs[i]);
                }
                logFile.WriteLine("Command dlls removed from command directory prior to command compilation!");
                //ServerConsole.newConsole.WriteLine("Command dlls removed from command directory prior to command compilation!");
            }
            else
            {
                logFile.WriteLine("There were no dlls present in the command directory to delete.");
                //ServerConsole.newConsole.WriteLine("There were no dlls present in the command directory to delete.");
            }

        }

        /// <summary>
        /// Gather up all the scripts that are going to be compiled
        /// </summary>
        /// <param name="scriptsToLoad"></param>
        /// <param name="commandDir"></param>
        public void CompileCommands(string scriptsToLoad, string commandDir)
        {
            if (scriptsToLoad == "all")
            {
                string[] allScripts = Directory.GetFiles(Path.Combine(Config.scriptDir, "all"), "*.cs", SearchOption.TopDirectoryOnly);
                CompileCommands(allScripts, commandDir);
            }
            else
            {
                List<string> files = new List<string>();
                string[] allScripts = Directory.GetFiles(Path.Combine(Config.scriptDir, "all"), "*.cs", SearchOption.TopDirectoryOnly);
                if (allScripts.Length > 0)
                {
                    files.AddRange(allScripts);
                }
                string[] gameScripts = Directory.GetFiles(Path.Combine(Config.scriptDir, scriptsToLoad), "*.cs", SearchOption.TopDirectoryOnly);

                for (int i = 0; i < gameScripts.Length; i++)// (string file in gameScripts)
                {
                    //for each script in the server script directory
                    FileInfo fileInfo = new FileInfo(gameScripts[i]);
                    string gameFile = fileInfo.Name;
                    //now we have the name of the script in the game directory
                    gameFile = gameFile.Remove(gameFile.IndexOf("."));

                    //check that against the names of the scripts in the all directory
                    //if no match, add it
                    //if match, replace it
                    for (int j = 0; j < files.Count; j++)// (string baseScript in files)
                    {
                        //check against the script that is in the list already from all
                        FileInfo allInfo = new FileInfo(files[j]);
                        string allFile = allInfo.Name;
                        allFile = allFile.Remove(allFile.IndexOf("."));
                        //now we have the name of the script in the all directory
                        if (allFile == gameFile)
                        {
                            //if there is a match, overwrite
                            files[j] = gameFile;
                            break;
                        }
                    }
                    //if there was no match, add it
                    files.Add(gameScripts[i]);
                }
                //once we get through all the scripts
                string[] scripts = files.ToArray();
                CompileCommands(scripts, commandDir);
                //                CompileCommands()
            }
        }

        /// <summary>
        /// From the list of all scripts files to compile, do the actual compilation
        /// </summary>
        /// <param name="scriptFiles"></param>
        /// <param name="commandDir"></param>
        public void CompileCommands(string[] scriptFiles, string commandDir)// string scriptDir, string commandDir)
        {
            //scripts from the all directory
            //string[] allScripts = Directory.GetFiles(Path.Combine(Config.scriptDir, "all"), "*.cs", SearchOption.TopDirectoryOnly);


            //load the scripts from the all directory AND the specific game server directory
            //               String[] scriptFiles = Directory.GetFiles(Path.Combine(Config.scriptDir, scriptsToLoad), "*.cs", SearchOption.TopDirectoryOnly);

            //String[] tempFiles = Directory.GetFiles()
            //String[] scriptFiles = Directory.GetFiles(scriptDir, "*.cs", SearchOption.TopDirectoryOnly);



            if (scriptFiles.Length == 0)
                return;//no files to compile

            int success = 0;
            int fail = 0;

            for (int i = 0; i < scriptFiles.Length; i++)
            {
                FileInfo fileInfo = new FileInfo(scriptFiles[i]);
                string fileName = fileInfo.Name;
                fileName = fileName.Remove(fileName.IndexOf("."));
                string dllFile = fileName + ".dll";
                string outputFile = Path.Combine(commandDir, dllFile);
                //ServerMain.newConsole.WriteLine(Assembly.GetExecutingAssembly().Location);

                //ServerMain.newConsole.WriteLine(scriptFiles[i]);
                string scriptSource = File.ReadAllText(scriptFiles[i]);

                SyntaxTree parsedSource = CSharpSyntaxTree.ParseText(scriptSource);
                string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

                MetadataReference[] references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Net.Sockets.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "Microsoft.CSharp.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.Expressions.dll")),
                    //MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Dynamic.Runtime.dll")),
                    //MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.CSharp")).Location),
                    //MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location)//this SHOULD give us access to everything public in this project
                    //MetadataReference.C
                };

                CSharpCompilation compilation = CSharpCompilation.Create(
                    dllFile,
                    syntaxTrees: new[] { parsedSource },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                EmitResult result = compilation.Emit(outputFile);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        //Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        logFile.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                    fail++;
                    File.Delete(outputFile);
                }
                else
                {
                    success++;
                }
            }

            if (success > 0)
            {
                logFile.WriteLine($"{success} commands compiled successfully");
                //ServerConsole.newConsole.WriteLine($"{success} commands compiled successfully");
            }
            if (fail > 0)
            {
                logFile.WriteLine($"{fail} commands failed to compile");
                //ServerConsole.newConsole.WriteLine($"{fail} commands failed to compile");
            }
            //logFile.WriteLine($"{success} commands compiled successfully");
            //logFile.WriteLine($"{fail} commands failed to compile");
            //ServerConsole.newConsole.WriteLine($"{success} commands compiled successfully");
            //ServerConsole.newConsole.WriteLine($"{fail} commands failed to compile");
        }

        public void LoadCommandDLL(string serverDirectory)
        {
            //ServerConsole.newConsole.WriteLine("Start loading dll files...");
            ConsoleOutput.SendConsole("Start loading dll files...");

            var allFilenames = Directory.EnumerateFiles(serverDirectory).Select(p => Path.GetFileName(p));
            var commandDLL = allFilenames.Where(fn => Path.GetExtension(fn) == ".dll").ToArray();
            //   .Select(fn => Path.GetFileNameWithoutExtension(fn)).ToArray();

            //      for (int i = 0; i < commandDLL.Length; i++)
            //    {
            //      ServerConsole.newConsole.WriteLine(commandDLL[i]);
            // }
            //ServerConsole.newConsole.WriteLine(commandDLL.Length);
            //ServerConsole.newConsole.WriteLine("blah");
            if (commandDLL.Length < 1)
            {
                //no files got loaded...
                logFile.WriteLine("No command dlls loaded!");
                //ServerConsole.newConsole.WriteLine("No command dlls loaded!");
                return;
            }
            //not sure we need to do this, since after this function is done, the reference to the dll file is gone and all the commands are loaded into memory?
            File.Copy(commandDLL[0], Path.Combine(serverDirectory, commandDLL[0] + ".loaded"), true);

            //        String[] commandDLLFiles = Directory.GetFiles(dllDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            //        if (commandDLLFiles.Length == 0)
            //            return;

            int loadedCommands = 0;//starting at 1 because of trial command, not anymore

            Assembly comAssem = commandAssembly.LoadFromAssemblyPath(Path.Combine(serverDirectory, commandDLL[0] + ".loaded"));//load the dll file
            Type[] dllClass = comAssem.GetExportedTypes();//get all the public classes from the loaded dll file

            logFile.WriteLine($"Command dll for {Config.config.gameName} loaded!");
            //ServerConsole.newConsole.WriteLine($"Command dll loaded!");
            foreach (var classType in dllClass)
            {
                //ServerConsole.newConsole.WriteLine(classType);
                var foundClass = (ICommand)Activator.CreateInstance(classType);
                if (foundClass == null)
                    continue;//if this is null, stop this loop and try the next

                //this will be if we have multiple aliases from a command, add the class to each alias in the command list
                //newCommand.commandClass = foundClass;
                //string[] aliases = foundClass.Aliases;

                foreach (string alias in foundClass.Aliases)
                {
                    Commands newCommand = new Commands();
                    newCommand.commandClass = foundClass;
                    newCommand.commandAlias = alias;
                    //ServerConsole.newConsole.WriteLine(alias + " " + foundClass);
                    commandList.Add(newCommand);
                }
                loadedCommands++;
            }
            //loadedCommands++;

            //ServerConsole.newConsole.WriteLine($"{commandDLLFiles.Length} dll files in directory!");
            /*           for (int i = 0; i < commandDLLFiles.Length; i++)
                       {
                           //ServerConsole.newConsole.WriteLine($"Loading: {commandDLLFiles[i]}");
                           //Assembly comAssem = commandAssembly.LoadFromAssemblyPath(commandDLLFiles[i]);
                           //var executeType = comAssem.GetType("Execute");
                           //Type[] dllClass = comAssem.GetExportedTypes();

                           //Commands newCommand = new Commands();
                           foreach (var classType in dllClass)
                           {
                               var foundClass = (ICommand)Activator.CreateInstance(classType);
                               if (foundClass == null)
                                   continue;//if this is null, stop this loop and try the next

                               //this will be if we have multiple aliases from a command, add the class to each alias in the command list
                               //newCommand.commandClass = foundClass;
                               foreach (string alias in foundClass.Aliases)
                               {
                                   Commands newCommand = new Commands();
                                   newCommand.commandClass = foundClass;
                                   newCommand.commandAlias = alias;
                                   commandList.Add(newCommand);
                               }

                           }
                           //ServerConsole.newConsole.WriteLine(blah[0]);
                           //newCommand.commandClass = (ICommand)Activator.CreateInstance(blah[0]);
                           //newCommand.commandName = newCommand.commandClass.ToString().ToLower();
                           //newCommand.commandClass.server = this;
                           //dynamic commandInstance = Activator.CreateInstance(blah[0]);
                           //commandList.Add(newCommand);// commandInstance.ToString().ToLower());
                           //ServerMain.newConsole.WriteLine(commandInstance.ToString().ToLower());
                           //commandInstance.server = this;
                           //commandInstance.Execute();
                           loadedCommands++;
                       }*/

            //commandList.Sort();
            //            foreach (Commands command in commandList)
            //            {
            //                ServerConsole.newConsole.Write(command.commandAlias);
            //            }
            commandList = commandList.OrderBy(x => x.commandAlias).ToList();//this will be the sort when we might actually have a class as the list instead of just string
                                                                            //           for (int i = 0; i < commandList.Count; i++)
                                                                            //           {
                                                                            //               ServerMain.newConsole.WriteLine(commandList[i]);

            //};
            //            ServerConsole.newConsole.WriteLine("sorted");
            //            foreach (Commands command in commandList)
            //            {
            //                ServerConsole.newConsole.Write(command.commandAlias);
            //            }
            logFile.WriteLine($"{loadedCommands} commands loaded into command list!");
            logFile.WriteLine($"{commandList.Count} total command aliases in list!");
            //ServerConsole.newConsole.WriteLine($"{loadedCommands} commands loaded into command list!");
            //ServerConsole.newConsole.WriteLine($"{commandList.Count} total command aliases in list!");
            //for (int i = 0; i < commandList.Count; i++)
            // {
            //   ServerConsole.newConsole.Write(commandList[i].commandAlias + " ");
            // ServerConsole.newConsole.WriteLine();
            // }
        }

        public async Task CharacterLogoutTask()
        {
            doLogouts = true;
            while (doLogouts)//while true? maybe some other bool might be better here
            {
                //Console.WriteLine("test");
                //check tasks are 
                //if (serverHasStarted)
                //{
                //Console.WriteLine(connections.Count);
                if (connections.Count > 0)// clientTasks.Count > 0)
                {
                    //go backwards through list?
                    for (int i = connections.Count - 1; i >= 0; i--)
                    //for (int i = 0; i < connections.Count; i++)// clientTasks.Count; i++)
                    {
                        //Console.WriteLine(connections[i].clientTask.Status.);
                        if (!ServerFunctions.IsClientStillConnected(connections[i].player))// connections[i].clientTask.Status != TaskStatus.WaitingForActivation && connections[i].clientTask.Status != TaskStatus.Running)// clientTasks[i].Status != TaskStatus.Running)//if the status of this task is not running (do we need to check for anything else? if it's anything else besides running, something's wrong?)
                        {//TaskStatus - only options for 'done' are Canceled, Faulted or RanToCompletion. 
                            logFile.WriteLine($"{connections[i].player.IP} client disconnected.");
                            //TcpClient tempClient = clientTasks[i].
                            //see if we can get the remoteendpoint of socket after it's closed
                            //Console.WriteLine(connections[i].client.Client.RemoteEndPoint);
                            RemoveConnection(connections[i].player);//  connections[i].client);
                        }
                        else
                        {
                            //Console.WriteLine("doh");
                        }
                    }
                    //      }
                    //      else
                    //      {
                    //Console.WriteLine("blahblah");
                    //      }
                    //Console.WriteLine("waiting");
                    //Thread.Sleep(1000);//pause for 1 second every loop
                    //await Task.Delay(1000);
                }
                else
                {
                    //Thread.Sleep(1000);//wait for a second
                    //await Task.Delay(1000);
                }
                await Task.Delay(1000);
            }
        }

        public void DoCommand(Player player, string commandMessage)//  Guid clientGUID, string commandMessage)// TcpClient client, string commandMessage)
        {
            //int commandIndex = -1;
            //string command = "";
            string[] message = commandMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);//just doing this here to get the command match, sending the full commandMessage to the command
            string commandToCheck = message[0].ToLower();
            ICommand? command = null;

            //if (!Temps.CheckLists(player, message))
            //  return;

            //I REALLY NEED TO REWORK THIS TO GET THE > COMMAND PROMPT TO SHOW CORRECTLY...I think I have?

            //           if (commandList.Count == 0)
            //           {
            //no commands in list so obviously there's going to be no match
            //               ServerFunctions.SendData(player, "<br>Unknown command.");
            //ServerFunctions.SendData(player, ">");
            //               return;
            //           }
            //ServerFunctions.SendData(player, $"({commandToCheck})");
            //for (int i = 0; i < commandList.Count; i++)
            // {
            //   ServerConsole.newConsole.Write(commandList[i] + " ");
            // ServerConsole.newConsole.WriteLine();
            // }
            //first check: check if message[0] matches any full command from list: ie, seeing if any of the specific shortened aliases are the first message
            //examples: ' for say, n,e,s,w,ne,se,sw,nw for move (direction), etc.

            foreach (Commands alias in commandList)
            {
                if ((alias.commandAlias.StartsWith(commandToCheck) || commandToCheck.ToLower() == alias.commandAlias) && player.AccountType >= alias.commandClass.AllowedUsers)//commandToCheck.ToLower() == alias.commandAlias || 
                {
                    //if our shortened, typed command matches the start of one of the command aliases, or our full typed command matches the full command alias
                    if (commandToCheck.StartsWith("'") && alias.commandAlias == "say")
                    {
                        //if the player started their command with the command shortcut ' and we're at the commandAlias say (meaning, the say command is in the list)
                        //then set the matched command to the say command
                        command = alias.commandClass;
                        break;
                    }

                    //ServerFunctions.SendData(player, $"match: {alias.commandAlias}");
                    command = alias.commandClass;
                    break;
                }
            }

            /*           foreach (Commands alias in commandList)
                       {
                           //ServerFunctions.SendData(player, alias.commandAlias);
                           //check to see if player is even allowed that command, if not don't even match it
                           if (commandToCheck.ToLower() == alias.commandAlias && player.AccountType >= alias.commandClass.AllowedUsers)
                           {
                               //check if there is a straight match between the typed command and an alias in the list
                               //ServerFunctions.SendData(player, alias.commandAlias);
                               command = alias.commandClass;
                               break;
                           }
                       }
                       //second check
                       //if we found a match already, skip this
                       //otherwise check commandToCheck against the list to see if we're using a shortened command
                       if (command == null)
                       {
                           foreach (Commands alias in commandList)
                           {
                               //ServerFunctions.SendData(player, alias.commandAlias);
                               //check to see if player is even allowed that command, if not don't even match it
                               if (alias.commandAlias.StartsWith(commandToCheck) && player.AccountType >= alias.commandClass.AllowedUsers)
                               {
                                   //ServerFunctions.SendData(player, alias.commandAlias);
                                   command = alias.commandClass;
                                   //ServerFunctions.SendData(player, "matched command");
                                   break;
                               }
                           }
                       }*/

            //if command is still null, no match found
            //or command is higher allowed than player is, then just show unknown instead of letting the user know that command exists, just in case
            //ie, admin commands

            //REDO THIS... being in roundtime doesn't matter for showing >, that's only going to matter for script commands from frontend
            //so, after doing a command, send > to show that command is done
            if (command == null && !ServerFunctions.IsPlayerInRT(player))//no command match found and not in RT
            {
                ServerFunctions.SendData(player, "<br>Unknown command.");
            }
            else if (command != null && (!ServerFunctions.IsPlayerInRT(player) || (ServerFunctions.IsPlayerInRT(player) && command.AllowedInRT)))
            {
                //if the player isn't in RT, OR the player IS in RT and the command is allowed in RT
                var room = ServerFunctions.GetRoom(player);//get the Room that the player is in from the cache/instanced area
                int roundTime = command.Execute(this, player, room, message);//this is the whole message, along with the typed command, in string[]
                                                                             //ServerFunctions.SendData(player, $"RT:{roundTime}");
                ServerFunctions.StartNewTimer(player, roundTime, "RT", "endRT");
            }
            else
            {
                //no match found and was in RT, or match found and can't do that command in RT
                ServerFunctions.SendData(player, "<br>wait for RT");
            }

            /*           if (command == null)// || player.AccountType < command.AllowedUsers)
                       {
                           ServerFunctions.SendData(player, "<br>Unknown command.");
                           //not do return? if unknown command, fall down to RT check?
                //           if (ServerFunctions.IsPlayerInRT(player))
                //           {
                               //if we're in RT and the player tries an invalid command, just send 'unknown command' then do nothing?
                //               return;
                //           }
                           //return;
                       } else
                       {
                           //command match found
                           if (!ServerFunctions.IsPlayerInRT(player) || (ServerFunctions.IsPlayerInRT(player) && command.AllowedInRT))
                           {
                               //if the player isn't in RT, OR the player IS in RT and the command is allowed in RT
                               var room = ServerFunctions.GetRoom(player);//get the Room that the player is in from the cache/instanced area
                               int roundTime = command.Execute(this, player, room, message);//this is the whole message, along with the typed command, in string[]
                               //ServerFunctions.SendData(player, $"RT:{roundTime}");
                               ServerFunctions.StartNewTimer(player, roundTime, "RT", "endRT");
                           } else
                           {
                               //we're in RT and the command isn't allowed?
                               ServerFunctions.SendData(player, "<br>wait for RT");
                               //return;
                           }
                       }*/
            //ServerFunctions.SendData(player, ">");

            //if (!ServerFunctions.IsPlayerInRT(player))
            // {
            //if the command we just finished didn't put the player into RT
            //     ServerFunctions.SendData(player, ">");
            //  }
        }

        public void RemoveConnection(Player player)// TcpClient client)
        {
            if (connections.Count == 0)
                return;

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].player == player)// client == client)
                {
                    //client.Close();
                    logFile.WriteLine($"{player.IP} {player.character.Name} logged out!");// connections[i].account.AccountName} logged out!");
                    connections.RemoveAt(i);

                }
            }
        }

        public void ProcessMessage(Player player, string fullMessage)//  Guid clientGUID, string message)// TcpClient client, string message)
        {
            //doing this on the ConnectedClient method since processmessage doesn't need it anyway
            // if (fullMessage.IndexOf("<EOF>") > -1)
            // {//this removes the <EOF> from the end of the message
            //   fullMessage = fullMessage.Remove(fullMessage.IndexOf("<EOF>"));
            // }

            string[] delimiter = { "::" };
            string[] splitMessage = fullMessage.Split(delimiter, StringSplitOptions.None);
            //int connectionIndex = -1;
            //Player player = null;
            if ((player.Guid != Guid.Empty && player.Guid.ToString() != splitMessage[0]) || player.Guid == Guid.Empty)//  player.Guid == Guid.Empty)
            {//if the player guid is not empty (not disconnected) and player guid and message guid don't match, wrong
                //if the player guid is empty (disconnected), wrong
                ServerFunctions.SendData(player, "<br>Invalid login. Please disconnect and retry login.");
                logFile.WriteLine("Somebody is connected with an empty guid!");
                return;
            }

            //index 0 is sender guid, will ignore from now on after this point
            //index 1 is message code, once checked no longer need it
            //index 2 is message/account name during login-050
            //index 3 is account password 1 during login-050
            //index 4 is password 2


            //Console.WriteLine(message);
            string code = splitMessage[1];// "0";
            string message = splitMessage[2];// "";
            //0 - Guid, 1 - code, 2 - clicklink trialverify, 3 - name, 4 - password1, 5 - password2
            //splitlink 0 - clicklink, splitlink 1 - trialverify
            //Guid clientGUID;
            //string[] delimiter = { "::" };
            //string[] splitMessage = fullMessage.Split(delimiter, StringSplitOptions.None);
            //Console.WriteLine(message);
            //           for (int i = 0; i < splitMessage.Length; i++)
            //           {
            //               Console.WriteLine(splitMessage[i]);
            //           }14 = 15.65 20 = 22.35  96.07813(font asset ascent + descent)/86 = 1.11718 * (font size) = preferred size in pixels rounded up to 2 decimal places
            //clientGUID = new Guid(splitMessage[1]);
            //code = splitMessage[1];
            //message = splitMessage[2];

            switch (code)
            {
                /*  case "000"://don't need this anymore since all the 'servers' are on the same process
                      logFile.WriteLine($"000 message from {connections[connectionIndex].clientIP}");
                      //game server connecting to login and sending auth string
                      //or login server sending game server their new guid
                      if (serverType == "login")
                      {//game server connecting to this login server
                          //string? gameServer = Config.instance.CheckServerName(splitMessage[3]);
                          if (splitMessage[4] == Config.instance.ConfigInfo("auth") && gameServer != null)
                          {
                              logFile.WriteLine("auth message matched!");
                              Guid tempGuid = Guid.NewGuid();
                              //SetClientGuid(client, tempGuid);
  //                            SendData(clientGUID, "000", tempGuid.ToString());//removed serverguid
                          }
                          else
                          {
                              //auth message doesn't match so we need to shut down this connection

                          }
                      }
                      else//this is a game server getting a guid from login
                      {
                          serverGuid = new Guid(splitMessage[3]);

                      }
                      break;*/

                case "050"://user login from frontend client
                    //find match to splitMessage[1] in login account database here
                    logFile.WriteLine(player.IP + " connected!");//Remote, on this socket which is both ends, look at the far end of the connection, from the server point of view
                    //logFile.WriteLine(connections[connectionIndex].clientIP + " connected!");//Remote, on this socket which is both ends, look at the far end of the connection, from the server point of view
                    //Console.WriteLine(client.Client.RemoteEndPoint);
                    ServerFunctions.SendData(player, "<br>Connected! Logging in...", false);// clientGUID, "Connected! Logging in...");//removed serverguid

                    //ServerMain.conn.Open();
                    Account loginAccount = DB.DBAccess.GetLoginAccount(message);// splitMessage[2]);
                    //DataTable loginDT = DBAccess.GetLoginAccount(splitMessage[2]);
                    //Console.WriteLine(loginAccount.HasRows);

                    if (message == " ")
                    {//we sent a " " blank account name. new user?
                        logFile.WriteLine(player.IP + " sent a \' \' account name. possible new user?");
                        //logFile.WriteLine(connections[connectionIndex].clientIP + " sent a \' \' account name. possible new user?");
                        //SetClientAccountName(connections[connectionIndex].client, splitMessage[2]);//why would we set the account name to ' ' in the first place?
                        ServerFunctions.SendData(player, "<br>You are trying to login with a blank account name. Either it was an accident or you're a new user. If so, would you like to start a trial account?<br>(If you would like to start a new trial account, please <link=\"clicklink trialstart\">click here</link>, or for more information: <link=\"clicklink trialinfo\">click here</link>)", false);//  type TRIAL START or TRIAL INFO for more information.)");
                        //SendData(clientGUID, "Cannont login: blank account name. Either it was an accident or you're a new user. If so, would you like to start a trial account?<br>(If you would like to start a new trial account, please type /trial start or /trial info for more information.)<br>");
                        return;
                    }
                    if (loginAccount.AccountName == "moo")//default name, obviously new account
                    {//unknown account name. also possible new user?
                        logFile.WriteLine(player.IP + " " + message + " unknown account name!");
                        //SetClientAccountName(connections[connectionIndex].client, splitMessage[2]);// somehow get the account name they sent to the new trial bit?
                        ServerFunctions.SendData(player, "<br>Unknown account. Either disconnect and check for typos or would you like to start a trial account?<br>(If you would like to start a new trial account, please <link=\"clicklink trialstart\">click here</link>, or for more information: <link=\"clicklink trialinfo\">click here</link>)\"", false); // type TRIAL START or TRIAL INFO for more information.)");
                        return;
                    }
                    if (message.Length < 8)
                    {//invalid account name
                        logFile.WriteLine(player.IP + " " + message + " invalid account name!");
                        ServerFunctions.SendData(player, "<br>Cannot login: invalid account name. How did you even accomplish that?", false);
                        return;
                    }
                    if (!Config.Verify(splitMessage[3], loginAccount.HashedPassword))// loginDT.Rows[0]["hashedPassword"].ToString()))
                    {//password doesn't match
                        logFile.WriteLine(player.IP + " " + message + " incorrect password.");
                        //ServerFunctions.SendData(player, splitMessage[3]);
                        ServerFunctions.SendData(player, "<br>Incorrect password. Please disconnect, check your password for typos and try again.", false);
                        return;
                    }
                    // if (loginAccount != null)// loginDT.Rows.Count > 0)
                    // {
                    //we already did a check for the default name so this is a matched account
                    logFile.WriteLine(player.IP + " " + message + " logged in!");
                    ServerFunctions.SendData(player, "052", $"account::{message}");
                    player.AccountType = loginAccount.AccountType;
                    //loginAccount.Id is the account Id, the player.Id is the Id for the character they might choose to play
                    // so we need to set the loginAccount.Id to the player's AccountId, not the player.Id
                    //since the AccountId is what we're checking in DoLogin to see if we've connected with a valid account or not (ie, if they're going through the new account process)
                    player.AccountID = loginAccount.Id;
                    //SetClientAccount(clientGUID, loginAccount);//we're logged in so set the connection account to the matched account     connections[connectionIndex].client, splitMessage[2]);
                    //ServerFunctions.SendData(player, "Logged in!<br>");//adding an additional br to the one we always add on the frontend so we skip a line

                    //if (loginDT.Rows[0]["accountType"].ToString() == "admin" || loginDT.Rows[0]["accountType"].ToString() == "mod")
                    //                    if (loginAccount.AccountType >= AccountType.Mod)//   loginAccount.AccountType == "admin" || loginAccount.AccountType == "mod")
                    //                    {
                    //                        ServerFunctions.SendData(player, "Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.");
                    //                        ServerFunctions.SendData(player, "Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>");
                    //                    }
                    //ServerMain.newConsole.WriteLine(loginDT.Rows[0]["accountID"]);
                    //check for characters here

                    //Int32.TryParse(loginDT.Rows[0]["accountID"].ToString(), out int accountID);

                    /*                   List<Player> characters = DBAccess.GetCharacters(loginAccount.AccountID);//  GetCharListForAccount(loginAccount.AccountID);
                                       //DataTable charList = DBAccess.GetCharListForAccount(loginAccount.AccountID);// accountID);
                                       ServerFunctions.SendData(player, "Current characters:");
                                       if (characters.Count > 0)//  charList.Rows.Count > 0)
                                       {//current characters on this account
                                           //SendData(clientGUID, "Current characters:");
                                           for (int i = 0; i < characters.Count; i++)//  charList.Rows.Count; i++)
                                           {
                                               //connections[connectionIndex].account.Characters.Add(characters[i].CharacterID);//not sure I need this?
                                               ServerFunctions.SendData(player, $"   {i + 1}) {characters[i].CharName}");//  charList.Rows[i]["charName"]}");
                                           }
                                       }
                                       else
                                       {//no characters created on this account
                                           ServerFunctions.SendData(player, "   (no characters created)");
                                       }
                                       ServerFunctions.SendData(player, $"   {characters.Count + 1}) Create new character.<br>");

                                       //ServerFunctions.SendData(player, "090", "character1,Blahblahmoomoomoo");
                                       //ServerFunctions.SendData(player, "<link=\"character1\"></link>");
                                       ServerFunctions.SendData(player, "Doh<link=\"character1\">Blahblahmoomoomoo</link>blah");
                                       //  }
                                       */
                    /*                  if (splitMessage[2].Length >= 8 && loginDT.Rows.Count > 0)//  loginAccount.Read())//gonna make account names be at least 8 characters long, if not invalid
                                      {//there is an account found in the database
                                          //SendData(client, serverGuid, "110", "Account name found");
                                          //check password
                                          //Console.WriteLine(loginAccount["accountName"].ToString());
                                          //ServerMain.newConsole.WriteLine(loginDT.Rows[0].ToString());
                                          if (Config.Verify(splitMessage[3], loginDT.Rows[0]["hashedPassword"].ToString()))// loginAccount["hashedPassword"].ToString()))
                                          {
                                              //passwords match!
                                              logFile.WriteLine(connections[connectionIndex].clientIP + " " + splitMessage[2] + " logged in!");
                                              SetClientAccountName(connections[connectionIndex].client, splitMessage[2]);
                                              SendData(clientGUID, "Logged in!<br>");//adding an additional br to the one we always add on the frontend so we skip a line

                                              if (loginDT.Rows[0]["accountType"].ToString() == "admin" || loginDT.Rows[0]["accountType"].ToString() == "mod")
                                              {
                                                  SendData(clientGUID, "Warning! Please note that \'characters\' created on admin or mod accounts are not to be used as normal player characters. Use a regular player account as these are meant for admin/mod duties.");
                                                  SendData(clientGUID, "Admin/mod characters are NOT server-specific so they can go anywhere they need to.<br>");
                                              }
                                              //ServerMain.newConsole.WriteLine(loginDT.Rows[0]["accountID"]);
                                              //check for characters here

                                              //int accountID;
                                              Int32.TryParse(loginDT.Rows[0]["accountID"].ToString(), out int accountID);//  1;// loginDT.Rows[0]["accountID"];
                                              //logFile.WriteLine(loginDT.Rows[0]["accountID"]);
                                              DataTable charList = DBAccess.instance.GetCharListForAccount(accountID);
                                              if (charList.Rows.Count > 0)
                                              {
                                                  SendData(clientGUID, "Current characters:");
                                                  for (int i = 0; i < charList.Rows.Count; i++)
                                                  {
                                                      SendData(clientGUID, $"   {i + 1}) {charList.Rows[i]["charName"]}");
                                                  }
                                              } else
                                              {
                                                  SendData(clientGUID, "   (no current characters created)");
                                              }
                                              SendData(clientGUID, $"   {charList.Rows.Count + 1}) Create new character.<br>");

                                              SendData(clientGUID, "090", "character1,Blahblahmoomoomoo");//removed serverguid
                                              SendData(clientGUID, "<link=\"character1\"></link>");//removed serverguid

                                          } else
                                          {
                                              //passwords don't match!
                                              logFile.WriteLine(connections[connectionIndex].clientIP + " " + splitMessage[2] + " incorrect password.");
                                              SendData(clientGUID, "Incorrect password.");//removed serverguid
                                          }
                                      } else
                                      {//no match found in the login database, let user know and possible start whatever to setup trial account if they want?
                                          logFile.WriteLine(connections[connectionIndex].clientIP + " " + splitMessage[2] + " unknown account!");
                                          SendData(clientGUID, "Unknown account. Either disconnect and check for typos or would you like to start a trial account?<br>(If you would like to start a new trial account, please type /trial start or /trial info for more information.)");//removed serverguid
                                          //Console.WriteLine("Unknown account name!");
                                      }*/

                    //ServerMain.conn.Close();
                    break;
                case "055":
                    //frontend to server - send new account info to verify (check account name and passwords)
                    //player.tempScript.Check(player, splitMessage[2], splitMessage[3], splitMessage[4]);
                    //moved this to 105 clicklink trialverify and trialconfirm

                    /*                   string tempAccountName = splitMessage[2];
                                       string tempPassword1 = splitMessage[3];
                                       string tempPassword2 = splitMessage[4];

                                       //do verification checks
                                       if (tempAccountName.Length < 8)
                                       {
                                           ServerFunctions.SendData(player, "Your account name needs to be at least 8 characters long. Please try again.");
                                           //break;
                                           return;
                                       }
                                       bool isUsedAccountName = DB.DBAccess.CheckAccountName(tempAccountName);

                                       if (isUsedAccountName)
                                       {
                                           //name found, can't use
                                           ServerFunctions.SendData(player, $"{tempAccountName} is not a valid new account name. Please choose another one.");
                                           //break;
                                           return;
                                       }
                                       if (tempPassword1 != tempPassword2)
                                       {
                                           //if the two entered passwords don't match
                                           ServerFunctions.SendData(player, "Your entered passwords don't match. Please re-enter them.");
                                           //break;
                                           return;
                                       }

                                       //if we got to this point, account name is acceptable and passwords match
                   //                    Account = name;
                   //                    Password = password1;
                                       //                    ServerFunctions.SendData(player, "054", "close");//don't close this just yet, they have to hit 'confirm'
                                       ServerFunctions.SendData(player, "You have picked a valid account name and your passwords match. If you would like to use these as your login, please click the 'confirm' button.");// type TRIAL ACCEPT or if you want to redo these, type TRIAL REDO");
                                       ServerFunctions.SendData(player, "060", "trialconfirm::open");*/
                    break;
                case "100"://command sent from user to server
                    //this gets added to queue
                    logFile.WriteLine($"{player.IP}: \"{message}\"");
                    //echo command back to player, no?
                    //ServerFunctions.SendData(player, $">{message}");

                    //check player.tempScript somehow?
                    //                   if (player.tempScript != null)
                    //                   {
                    //player.tempScript.MakeNewTrial(player); //doesn't error out, at least
                    //                   }
                    //SendData(client, serverGuid, "110", "Message received...");
                    //SendData(client, serverGuid, "092", "blah");//works!!!
                    ServerFunctions.AddCommandToQueue(player, message);
                    //CommandQueue commandTyped = new CommandQueue();
                    //commandTyped.player = player;
                    //commandTyped.commandMessage = message;
                    //commandQueue.Add(commandTyped);
                    //                    DoCommand(player, message);//  clientGUID, splitMessage[2]);//'client' will need to be changed to user guid at some point
                    break;

                case "105":
                    //ServerFunctions.SendData(player, $"Link id {message} clicked!");
                    //this gets added to queue
                    //do checks to see if the incoming clicked link is a command or an item/object reference?
                    //since textmeshpro link ids can use spaces, we'll use the ids as commands so we can send 'choose 1' instead of choose1
                    if (message.StartsWith("clicklink"))
                    {
                        //clicked link from frontend that isn't a normal command (open objects, character/server select, etc)
                        string[] splitLink = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        int option = 0;

                        switch (splitLink[1])//since splitLink[0] is "clicklink"
                        {
                            case "charselect":
                                List<Character> characterList = DB.DBAccess.GetCharacters(player, Config.config.gameName);
                                //try to convert splitLink[2], ie: the select char option to an int
                                if (!Int32.TryParse(splitLink[2], out option))
                                {
                                    ServerFunctions.SendData(player, "<br>An error occured. Please select a character.", false);
                                    ServerFunctions.ShowCharSelection(player);
                                    return;
                                }
                                //put check to see if we're at max character count here?
                                //to see if 'new character' is a valid option or not

                                if (option > -1 && option <= characterList.Count)
                                {
                                    // if we got the option clicked (greater than -1) and it's less than the char.count
                                    //since 0-3 is less than count of 4
                                    //load character and all that stuff
                                    //or new character
                                    if (characterList[option].Id != Guid.Empty)
                                    {
                                        //if this is an already created character
                                        //we already have the Character class for this character loaded in the list so we just need to copy it to the player
                                        //ServerFunctions.UpdateCharOnPlayer(player, characterList[option]);
                                        //testing
                                        //player = (Player)characterList[option];
                                        //ServerFunctions.SendData(player, $"testing: {player.Guid.ToString()},{player.AccountType},{player.Name}");
                                        player.character = characterList[option];
                                        //ServerFunctions.UpdateObject(player, characterList[option]);
                                        //player.character = characterList[option];

                                        //player.tempScript = null;
                                        player.SendData("052", $"character::{player.character.Name}");
                                        //ServerFunctions.SendData(player, "052", $"character::{player.character.Name}");
                                        //if somehow, current room is guid.empty, move them to first room? which should be char creation. but if they have
                                        //a name, they should be moved to room that charcreation drops them into, once we get to that point of course
                                        //ServerFunctions.SendData(player, player.CurrentRoom.ToString(), false);
                                        if (player.character.CurrentRoom == Guid.Empty)
                                        {
                                            //ServerFunctions.SendData(player, "moo?");
                                            Room charGen = ServerFunctions.GetRoom(player, Config.config.FirstRoom);// player.CurrentServer.serverInfo.FirstRoom);
                                            //Room charGen = DB.DBAccess.GetById<Room>(player.CurrentServer.firstRoom, DB.Collections.Room);//  ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                                            //ServerFunctions.SendData(player, "moo?");
                                            ServerFunctions.AddCommandToQueue(player, $"move {charGen.Id}");
                                            //Region charReg;
                                            //      ServerFunctions.SendData(player, "moo?");
                                            //    for (int i = 0; i < ServerFunctions.servers.Count; i++)
                                            //  {
                                            //    ServerFunctions.SendData(player, ServerFunctions.servers[i].connections.Count.ToString());
                                            // }
                                            //ServerFunctions.SendData(player, "no region,or not instanced");
                                            //player.CurrentRoom = player.CurrentServer.firstRoom;
                                        }

                                        //ServerFunctions.SendData(player, $"Selected option #{menuOption}");
                                        //var character = DB.DBAccess.GetById<Character>(characterList[menuOption - 1].Id, DB.Collections.Character);
                                        //player = (Player)character;//  JsonConvert.DeserializeObject<Character>(JsonConvert.SerializeObject(data));
                                        //ServerFunctions.SendData(player, $"{(Races)character.Race}");
                                        //ServerFunctions.AddCommandToQueue(player, "look");//this added to end of 'move' command so we always look after a move
                                    }
                                    else
                                    {
                                        //new character
                                        //get the first room from the server info
                                        //player.tempScript = null;//this copy of CharSelect should still run until end of script? then be GC'd later
                                        Room charGen = DB.DBAccess.GetById<Room>(Config.config.FirstRoom, DB.Collections.Room);//  ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                                        ServerFunctions.SendData(player, "moo?");
                                        ServerFunctions.AddCommandToQueue(player, $"move {charGen.Id}");

                                        //ServerFunctions.SendData(player, "no region,or not instanced");
                                        //     player.CurrentRoom = player.CurrentServer.firstRoom;// ServerFunctions.servers[chosenServer].firstRoom;
                                        //player.tempScript = null;
                                        player.tempScript = new CharManager(player, player);
                                        ServerFunctions.AddCommandToQueue(player, "look");
                                    }
                                    return;

                                }
                                else
                                {
                                    ServerFunctions.SendData(player, "An error occured. Please select a character.");
                                    ServerFunctions.ShowCharSelection(player);
                                    //return;
                                }
                                break;

                            case "trialstart":
                                ServerFunctions.SendData(player, "Thank you for wanting to give this game a try! In order to get your trial account set up, please set your account name and password in the provided window.");
                                ServerFunctions.SendData(player, "052", "newaccount::open");
                                ServerFunctions.SendData(player, "052", "trialconfirm::close");
                                break;
                            case "trialnewcancel":
                                ServerFunctions.SendData(player, "Well, we're sad to see that you decided to not try our game but if you change your mind, please come on back.");
                                //disconnect
                                break;
                            case "trialverify":
                            case "trialconfirm":
                                //put both checks here so we don't screw things up by fixing one and not the other
                                //since the splitlink "clicklink trialverify/trialconfirm" takes up splitmessage2, the next 3 variables will be 3,4,5 instead of 2,3,4
                                string tempAccountName = splitMessage[3];
                                string tempPassword1 = splitMessage[4];
                                string tempPassword2 = splitMessage[5];

                                //do verification checks
                                if (tempAccountName.Length < 8 && tempAccountName.Length > 3)
                                {
                                    if (splitLink[1] == "trialverify")
                                    {
                                        ServerFunctions.SendData(player, "Your account name needs to be at least 8 characters long. Please try again.");
                                    }
                                    else
                                    {
                                        ServerFunctions.SendData(player, "You trying to pull a fast one on us by sneaking by a different account name? Try again.");
                                    }
                                    //break;
                                    return;
                                }
                                bool isUsedAccountName = DB.DBAccess.CheckAccountName(tempAccountName);

                                if (isUsedAccountName)
                                {
                                    //name found, can't use
                                    ServerFunctions.SendData(player, $"{tempAccountName} is not a valid new account name. Please choose another one.");
                                    //break;
                                    return;
                                }
                                if ((tempPassword1 != tempPassword2) && tempPassword1.Length > 0)
                                {
                                    //if the two entered passwords don't match
                                    ServerFunctions.SendData(player, "Your entered passwords don't match. Please re-enter them.");
                                    //break;
                                    return;
                                }

                                //checks done, all good
                                if (splitLink[1] == "trialverify")
                                {
                                    //show confirm button and text
                                    ServerFunctions.SendData(player, "You have picked a valid account name and your passwords match. If you would like to use these as your login, please click the 'confirm' button.");// type TRIAL ACCEPT or if you want to redo these, type TRIAL REDO");
                                    ServerFunctions.SendData(player, "052", "trialconfirm::open");
                                }
                                else
                                {
                                    //confirmed, create account
                                    //if (Account.Length > 0 && Password.Length > 0)
                                    //{
                                    //if the name and password are both longer than 0, meaning we've put something in there, then we're good?
                                    Account account = new Account();
                                    account.Id = DB.DBAccess.GetNewGuid<Account>(DB.Collections.Account);
                                    account.AccountName = tempAccountName;
                                    account.HashedPassword = Config.Hash(tempPassword1);
                                    account.AccountType = AccountType.Trial;
                                    List<Server> servers = DB.DBAccess.GetList<Server>(DB.Collections.Server);
                                    for (int j = 0; j < servers.Count; j++)
                                    {
                                        if (servers[j].IsDefault && servers[j].Type == "game")
                                        {
                                            account.AllowedServers += $",{servers[j].Name}";
                                        }
                                    }
                                    //got all the default servers, strip the first ','
                                    if (account.AllowedServers.IndexOf(',') == 0)
                                    {
                                        //if the first ',' is at index 0, remove just that
                                        account.AllowedServers = account.AllowedServers.Remove(0, 1);
                                    }
                                    ServerFunctions.SendData(player, "052", $"account::{account.AccountName}");
                                    DB.DBAccess.Save(account, DB.Collections.Account);
                                    //player.tempScript = null;
                                    player.AccountID = account.Id;
                                    //}
                                }

                                break;
                            case "trialinfo":
                                ServerFunctions.SendData(player, "A trial account lets you try out this game without having to register on the website. You are only able to create 1 character, although you can delete it and start another if you wish. The account will be active for 30 days, after which you can no longer play unless you register. If you haven't registered after 60 days from the start of the trial, the account and character will be deleted.");
                                ServerFunctions.SendData(player, "Some classes will be unavailable and some regions will be inaccessible.");
                                break;
                        }

                    }
                    else
                    {
                        //clicked link from frontend that is functionally the same as a manually-typed command
                        ServerFunctions.AddCommandToQueue(player, message);
                    }


                    //                   if (message.StartsWith("choose"))//since we're still dealing with "choose 1" or "choose blahblahblah" as the full string, not spaced out yet
                    //                   {//this is a command, so we can just send it on as a command. don't need to add anything to it
                    //                       ServerFunctions.AddCommandToQueue(player, message);
                    //CommandQueue commandClick = new CommandQueue();
                    //commandClick.player = player;
                    //commandClick.commandMessage = message;
                    //commandQueue.Add(commandClick);
                    //  DoCommand(player, message);
                    //break;
                    //                   }


                    /*          switch (message)
                              {
                                  case "newcharacter":
                                      //check firstRoom
                                      Room charGen = DB.DBAccess.GetById<Room>(firstRoom, DB.Collections.Room);
                                      //Region charReg;
                                      if (charGen.Region != Guid.Empty)
                                      {
                                          Region charReg = DB.DBAccess.GetById<Region>(charGen.Region, DB.Collections.Region);
                                          if (charReg != null && charReg.IsInstanced)
                                          {
                                              ServerFunctions.SendData(player, "this is a region instance");

                                              break;
                                          }
                                      }

                                      ServerFunctions.SendData(player, "no region,or not instanced");
                                      player.CurrentRoom = firstRoom;
                                      break;


                              }*/
                    break;

                case "10000"://user forcing a disconnect with their 'disconnect' button, not the same as doing a /logout
                    //logFile.WriteLine("client requesting to logout");
                    ServerFunctions.SendData(player, "10000", "<br>Warning! Forcing a disconnect from the game will not guarantee that your character will be logged out immediately.");//removed serverguid
                    //close this client
                    player.Client.Close();
                    //Thread.Sleep(1000);
                    break;
                default:
                    logFile.WriteLine($"{player.IP} unknown message code: {code}");
                    break;
            }
            //Console.WriteLine("message processed?");
        }

        public void StopServer()
        {
            //         if (serverType == "login")//don't need this since 'game' servers now handle their own listeners
            //           {//if the type is login
            //stop the listener
            //disconnect clients
            //'logout' characters ie: RemoveConnection()
            try
            {
                //ServerConsole.newConsole.WriteLine("login listener stopped?");
                listener.Stop();
                listenerTask.Wait();
                logFile.WriteLine("listenertask is done");
            }
            catch (SocketException)
            {
                //newConsole.WriteLine("maybe here?");
                //there were people trying to connect when we closed the listener
            }

            //ServerConsole.newConsole.WriteLine(connections.Count);
            //if (connections.Count > 0)
            //{//start at the end and go back?
            //ServerConsole.newConsole.WriteLine("disconnecting players?");
            logFile.WriteLine($"Disconnecting {connections.Count} players...");
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                //ServerConsole.newConsole.WriteLine(connections[i].player.AccountID);
                //logFile.WriteLine(connections[i].player.IP + " disconnected?");
                ServerFunctions.SendData(connections[i].player, "10000", "You have been booted from the server due to the server shutting down. Please reconnect later. Thank you.");//  connections[i].client, "10000", "goodbye");//removed serverguid
                connections.RemoveAt(i);
            }
            //}
            doLogouts = false;
            checkRTs = false;
            doRandom = false;
            logoutTask.Wait();
            roundtimeCheck.Wait();
            //randomOutput.Wait();
            //logFile.WriteLine($"logout task done");
            connections.Clear();
            commandList.Clear();
            //newTrialAccounts.Clear();
            commandQueue.Clear();
            commandAssembly.Unload();

            //commandAssembly.Assemblies);
            //                commandAssembly.Unload();//shouldn't need this for login server
            //ServerConsole.newConsole.WriteLine(commandAssembly.Name);
            commandAssembly = null;
            //ServerConsole.newConsole.WriteLine(commandAssembly.Name);
            //         }
            //         else
            //         {//for game servers



            //         }
            //ServerConsole.newConsole.WriteLine("doh?");
            //logFile.WriteLine($"lists are cleared");
            logFile.Close();
            serverIsRunning = false;
        }

        public bool CommandsInQueue()
        {
            return commandQueue.Count > 0;
        }

        public Room GetRoom(Player player)
        {
            if (player.RegionInstance != Guid.Empty)
            {
                //if the player is in a region instance, get the room from that instance
                for (int i = 0; i < regions.Count; i++)
                {
                    //find the instance that this player is in?
                    if (regions[i].regionInstance == player.RegionInstance)
                    {
                        for (int j = 0; j < regions[i].rooms.Count; j++)
                        {
                            //find the room in this instance that the player in is
                            if (regions[i].rooms[j].Id == player.character.CurrentRoom)
                            {
                                return regions[i].rooms[j];
                            }
                        }
                    }
                }
            }
            else
            {
                //player is not in a region instance, so just get the room from the cache that everybody can get to
                for (int i = 0; i < roomCache.Count; i++)
                {
                    if (roomCache[i].Id == player.character.CurrentRoom)
                    {
                        return roomCache[i];
                    }
                }
            }

            ServerFunctions.SendToLogfile(player, $"{player.character.Name} somehow not found in GetRoom on {Config.config.gameName}!");
            return new Room();
        }

        //return type WAS SQLiteDataReader
        /*      public DataTable GetLoginAccount(SQLiteConnection conn, string accountName)// string connString, string accountName)
              {
                  //Console.WriteLine(connString);
                  //            using SQLiteConnection conn = new SQLiteConnection(connString);
                  //            conn.Open();

                  string loginAccount = "SELECT * from accounts WHERE accountName = @accountName";// {accountName}";

                  SQLiteCommand cmd = new SQLiteCommand(loginAccount, conn);
                  cmd.Parameters.AddWithValue("@accountName", $"{accountName}");
                  //Console.WriteLine(cmd);
                  //SQLiteDataReader data;
                  try
                  {
                      ServerMain.conn.Open();
                      SQLiteDataReader data = cmd.ExecuteReader(CommandBehavior.SingleResult);

                      DataTable loginData = new DataTable();
                      loginData.Load(data);
                      //ServerMain.conn.Close();
                      return loginData;
                      //Console.WriteLine("trying to find a match");
                      //return cmd.ExecuteReader(CommandBehavior.SingleResult);
                      //if (data.Read())
                      //{
                      //    Console.WriteLine(data["accountName"].ToString());
                      //Console.WriteLine("found record?");
                      //Console.WriteLine(data);
      //                return data;
                      //}
                  }
                  catch (SQLiteException)
                  {
                      Console.WriteLine("error when getting a reader result");
                      //conn.Close();
                      //return null;
                  }
                  finally
                  {
                      ServerMain.conn.Close();
                      //conn.Close();
                      //Console.WriteLine("leaving lookup");
                  }
                  //Console.WriteLine(cmd.ExecuteReader(CommandBehavior.SingleResult));
                  //Console.WriteLine(data);
                  //return cmd.ExecuteReader(CommandBehavior.SingleResult);
                  //conn.Close();
                  return null;
              }*/

        //this should be the clients that have been logged in and authenticated
        public class Connections
        {
            //public Task? clientTask = null;//this is the task for receiving data from client
            //public TcpClient? client = null;//the client-server socket// on the player class itself
            //public Account? account;//the account id is on the player class itself
            public Player player;
            //public string clientIP = "";//the logged IP of the client //on the player class itself
            //public string accountName = "";
            //public Guid guid;//connected server name or user guid goes here, default guid is Guid.Empty, all zeros // on the player class itself
            //public string accountType = "";//trial, basic, etc, mod, admin
            //public string characterName = "";//for after connection and picking a character, or creating a new one

        }

        public class NewTrial
        {
            //public TcpClient? client = null;//since we don't have an account name yet, have to use the client to check against
            public Player? player;
            //public Account account = new Account();
            public bool isReady = false;
            public string account = "";
            public string password = "";
            public string password1 = "";
            //public Guid clientGUID;//so we can check against the client guid and this new trial 'quest' to create a new trial account so we can keep track
            //public string account = "";
            //public string password = "";
            //public bool isAccountName = false;//when account name has been verified by user
            //public bool isPassword = false;//when password has been verified by user
        }

        public class CommandQueue
        {
            public Player player;//  Guid clientGuid;
            public string commandMessage;
        }

        public class Commands
        {
            public string commandAlias;
            public ICommand commandClass;

        }

        public class InstancedRegions
        {
            public Region Region;
            public Guid regionInstance;
            public Guid owner;//person who moved into this region and caused it to be spawned
            public Timer? isEmptyTimer; //somehow check for when all players are out of all rooms in this region, then start a timer. when done, clear the list of rooms, then remove this region from instances
            public List<Room> rooms = new List<Room>();
        }

        public class PlayerTimers
        {
            public Player player;
            public DateTime timerStart; //use DateTime.UtcNow
            public TimeSpan timerLength; //how long the timer is for: TimeSpan.FromSeconds(durationInSeconds);
            public string atTimerEnd; //what to do when the timer runs out
            public string timerType; //RT, room perc, etc
            public Guid targetGuid; //the guid for which character/room/whatever this timer is specific for
        }


    }
}
