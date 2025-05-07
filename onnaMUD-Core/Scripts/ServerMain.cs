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
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.Text;
using System.Reflection;
//using Microsoft.CodeAnalysis.Emit;
using System.Data;
using System.Collections.Concurrent;
using onnaMUD.Settings;
using onnaMUD.Database;
using onnaMUD.Utilities;
//using onnaMUD.Accounts;
using onnaMUD.Characters;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Numerics;
using System.IO;
using onnaMUD.Temps;
using System.Xml.Linq;
using System.IO.Pipes;
using Newtonsoft.Json.Linq;
using static onnaMUD.Settings.Config;
using static onnaMUD.Utilities.ServerConsole;

namespace onnaMUD.MUDServer
{
    public class ServerMain
    {
        public string serverSubDir = "";
        public string pluginDir = "";

        public int numOfBytes = 1024;

        private TcpListener? listener;
        private Task? listenerTask;
        private Task? logoutTask;
        private Task? roundtimeCheck;
        private Task? randomOutput;
        //private bool isListening = false;
        //private bool doLogouts = false;
        //private bool checkRTs = false;
        //private bool doRandom = false;
        //private bool checkQueue = false;

        //public ManualResetEvent serverResetEvent = new ManualResetEvent(false);
        public bool isServerCreated = false;
        public bool serverIsRunning = false;
        public bool doServerTasks = false;//use this for all the server task bools
        public bool waitForPipeClient = false;

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
        //public AssemblyLoadContext? commandAssembly;// = new AssemblyLoadContext("commands", true);
        public WeakReference assemWeak;

        public List<PlayerTimers> playerTimers = new List<PlayerTimers>();
        public List<Task> tasks = new List<Task>();
        public ConcurrentQueue<Task> taskQueue = new ConcurrentQueue<Task>();

        //cache stuff here to make it easier on the database?
        public List<Room> roomCache = new List<Room>();
        public NamedPipeServerStream? namedPipeServer = null;// new NamedPipeServerStream(Config.pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);// new NamedPipeServerStream(Config.pipeName, PipeDirection.InOut, 1);//, PipeTransmissionMode.Message);Message only works for windows?
        public ProcessPipe consolePipe;
        private Task? clientPipeTask = null;
        private bool isOutputConnected = false;
        public Task? serverPipeRead = null;
        private bool readFromPipe = false;
        //public TaskCompletionSource<bool> isOutputConnected = new TaskCompletionSource<bool>(false);

        //hopefully I'll eventually get to the point where the server class itself handles just the scheduling of tasks and other management/maintenance stuff
        //the ServerFunctions will have all the actual functions to do stuff
        public ServerMain(ProcessPipe processPipe)// string name, string type, bool doDebug)//  string serverToStart)// ServerMain.ServerInfo serverToStart)
        {
            isServerCreated = false;
            consolePipe = processPipe;

            //Make sure server subdirectories are created
            serverSubDir = Directory.GetCurrentDirectory();//  Path.Combine(Directory.GetCurrentDirectory(), Config.config.gameName);
            if (!Directory.Exists(Path.Combine(serverSubDir, "logs")))
            {
                //subdirectory for log files
                Directory.CreateDirectory(Path.Combine(serverSubDir, "logs"));
            }
            pluginDir = Path.Combine(serverSubDir, "plugins");
            if (!Directory.Exists(pluginDir))
            {
                //subdirectory for dll plugin files (commands, etc.)
                Directory.CreateDirectory(pluginDir);
            }

            //start up the named pipe server for console output
            // we do this first before making a new log file so if pipe fails, we don't make a new empty log file for no reason
            //no longer needed here, this is done in the processPipe that is made in Program before servermain is made
       /*     try
            {
                namedPipeServer = new NamedPipeServerStream(config.serverName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                taskQueue.Enqueue(WaitForPipeClient());
                //tasks.Add(WaitForPipeClient());
            }
            catch (IOException)
            {
                //tried to start another server instance
                return;
            }*/

            //if we get to this point, then this server hasn't been started yet, else the creation of another named pipe of same name would have errored out

            //start the log file so we can immediately start logging server output
 /*           try
            {
                string logFileName = GetNextLogFileName(Path.Combine(serverSubDir, "logs"), config.serverName);
                //SendToConsole(logFileName);
                logFile = new TimeStampedStream(logFileName);// GetNextLogFileName(Path.Combine(serverSubDir, "logs"), config.serverName));
                
                //StreamWriter errorLog = new StreamWriter(logFile);
                //StreamWriter errorLog = new TimeStampedErrorStream(logFileName);
                logFile.AutoFlush = true;
                Console.SetOut(logFile);
                //errorLog.AutoFlush = true;
                Console.SetError(new TimeStampedTextWriter());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }*/
            isServerCreated = true;
        }

        public void Log(string message)
        {
            //logFile.WriteLine("Caching rooms from database...");
            if (logFile != null)
            {
                logFile.WriteLine(message);
                logFile.Flush();
            }
        }

        /*           public Server(int serverListIndex)
                   {

                       //Process.GetCurrentProcess().EnableRaisingEvents = true;
                       //Process.GetCurrentProcess().Exited += ServerHasShutdown;// new EventHandler(ServerHasShutdown);
                   }*/


        public async Task StartServer()//  public void StartServer()
        {
            //start the log file so we can immediately start logging server output
            try
            {
                string logFileName = GetNextLogFileName(Path.Combine(serverSubDir, "logs"), config.serverName);
                //SendToConsole(logFileName);
                logFile = new TimeStampedStream(logFileName);// GetNextLogFileName(Path.Combine(serverSubDir, "logs"), config.serverName));

                //StreamWriter errorLog = new StreamWriter(logFile);
                //StreamWriter errorLog = new TimeStampedErrorStream(logFileName);
                logFile.AutoFlush = true;
                Console.SetOut(logFile);
                //errorLog.AutoFlush = true;
                Console.SetError(new TimeStampedTextWriter());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }

            doServerTasks = true;
            //          if (!Config.LoadConfig())
            //              return;//if we had to create a new appsettings.json file, exit so user can edit file as needed

            //gotta see if the db is there before we try to connect to it to do stuff
            consolePipe.SendToConsole("Checking for local database...");
            //logFile.WriteLine("Checking for local database...");
            Log("Checking for local database...");
            await Task.Delay(500);
            DB.CheckForLocalDB();
            consolePipe.SendToConsole("Local database loaded.");
            Log("Local database loaded");
            await Task.Delay(1000);
            consolePipe.SendToConsole("Setting account values...");
            Log("Setting account values...");
            //this is to input the account types and their settings into the list of account types
            for (int i = 0; i < Enum.GetValues(typeof(AccountType)).Length; i++)
            {
                switch (i)
                {
                    case 0://trial account
                        accountSettings.Add(new AccountSettings((AccountType)i, 1, 1));
                        break;


                }
            }
            await Task.Delay(500);
            consolePipe.SendToConsole("Account values set.");
            Log("Account values set.");
            await Task.Delay(1000);

            //Log($"Starting up {config.gameName} server...");
            //SendToConsole($"Starting up {Config.config.gameName} server...");
            //await Task.Delay(500);

            //logoutTask = CharacterLogoutTask();//this is what removes logged-in characters from the list after the client has/has been disconnected

            Log("Loading admin commands...");//eventually
            //ServerConsole.newConsole.WriteLine("Loading admin commands");
            consolePipe.SendToConsole("Loading admin commands...");
            await Task.Delay(500);
            LoadAdminCommands();
            //blah, load commands that are part of the core server, ie: admin commands
           // Log("Admin commands loaded");//eventually
            //ServerConsole.newConsole.WriteLine("Admin commands loaded");
          //  consolePipe.SendToConsole("Admin commands loaded");

            //load all dll files from /plugins directory. load all ICommand classes/aliases into command list, then sort after
            await Task.Delay(1000);
 //           commandAssembly = new AssemblyLoadContext("commands", true);
 //           assemWeak = new WeakReference(commandAssembly);

            consolePipe.SendToConsole("Start loading plugin files...");
            Log("Start loading plugin files...");
            await Task.Delay(500);
            LoadPluginDLL(serverSubDir);
            //ServerConsole.newConsole.WriteLine("Caching rooms from database...");

            await Task.Delay(1000);
            Log("Caching rooms from database...");
            //ServerConsole.newConsole.WriteLine("Caching rooms from database...");
            consolePipe.SendToConsole("Caching rooms from database...");
            await Task.Delay(500);

            //load the room DB. only add rooms that aren't instanced since those will be created on the fly?
            roomCache = DB.DBAccess.GetList<Room>(DB.Collections.Room);
            //ServerConsole.newConsole.WriteLine("Rooms loaded!");
            Log("Done loading rooms");
            //ServerConsole.newConsole.WriteLine("Done loading rooms");
            consolePipe.SendToConsole("Done loading rooms");
            await Task.Delay(1000);
            roundtimeCheck = CheckPlayerTimers();
            //randomOutput = RandomOutput();
            Log("Starting server tasks...");
            consolePipe.SendToConsole("Starting server tasks...");
            await Task.Delay(500);
            //listenerTask = StartListening();
            logoutTask = CharacterLogoutTask();//this is what removes logged-in characters from the list after the client has/has been disconnected

            taskQueue.Enqueue(CheckQueueForCommands());
            Log("Command queue started.");
            consolePipe.SendToConsole("Command queue started.");
            await Task.Delay(1000);

            taskQueue.Enqueue(StartListening());
            Log($"{config.gameName} {config.serverName} listening on port {config.port}");
            consolePipe.SendToConsole($"{config.gameName} {config.serverName} listening on port {config.port}");
            await Task.Delay(1000);

            Log($"{Config.config.gameName} server started!");
            //ServerConsole.newConsole.WriteLine($"{Config.config.gameName} server started!");
            consolePipe.SendToConsole($"{Config.config.gameName} server started!");
            await Task.Delay(1000);
            //                  break;

            //           }

            //logFile.WriteLine($"{serverName} server started!");
            //serverPipeRead = ReadFromPipe();
            //tasks.Add(WaitForPipeClient());
            //tasks.Add(ReadFromPipe());

            //start a task to keep the server running and return that task
            serverIsRunning = true;
            consolePipe.SendToConsole("60", "blah");
            
            //return ServerRunning();
        }

 /*       async Task WaitForPipeClient()
        {
            //this SHOULD wait for a connection, then do ReadFromPipe until connection is broke, then loop back and wait for another connection?
            waitForPipeClient = true;
            //Console.WriteLine("pipe waiting");
            while (waitForPipeClient)
            {
                try
                {
                    //Log("waiting for pipe client");
                    await namedPipeServer.WaitForConnectionAsync();
                    //Log("pipe client connected. reading...");
                    isOutputConnected = true;
                    //Log("blah?");
                    //SendToConsole("50", serverIsRunning.ToString());
                    await ReadFromPipe();
                    //Log("pipe client disconnected.");
                    isOutputConnected = false;
                    //clientPipeTask = ReadFromPipe();
                    //SendToConsole("connected");
                    //tasks.Add(ReadFromPipe());
                    //isOutputConnected.SetResult(true);
                }
                catch (Exception ex)
                {
                    //logFile.WriteLine($"WaitForPipeClient: {ex}");//if we're doing this at server startup, logfile isn't set yet so this errors
                    SendToConsole(ex.ToString());
                    //Console.ReadLine();
                    return;
                }
            }

        }*/

 /*       async Task ReadFromPipe()
        {
            //StreamReader pipeReading = new StreamReader(namedPipeServer);
            int numOfBytes = 1024;
            string receivedDataBuffer = "";
            byte[] bytes = new byte[numOfBytes];
            bool checkMessage = false;
            int eofIndex = 0;
            readFromPipe = true;

            try
            {
                //StreamReader pipeReading = new StreamReader(namedPipeServer);

                while (readFromPipe)
                {
                    try
                    {
                        //int charsRec = await pipeReading.ReadAsync(chars, 0, numOfChar);
                        int bytesRec = await namedPipeServer.ReadAsync(bytes, 0, numOfBytes);
                        if (bytesRec > 0)
                        {
                            receivedDataBuffer += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                            checkMessage = true;
                        }
                        else
                        {
                            readFromPipe = false;
                            namedPipeServer.Disconnect();
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
                        //Console.WriteLine(receivedStringBuffer);
                        if (receivedDataBuffer.IndexOf("::") == 5)
                        {
                            try
                            {
                                eofIndex = Int32.Parse(receivedDataBuffer.Substring(0, 5));//parse the first 5 characters for the index of EOF
                            }
                            catch (FormatException)
                            {
                               // SendToLogfile(player, $"{player.AccountID} has an invalid message index. Clearing data buffer.");

                                //player.SendData("There was an communications error. Please try again.");
                                //SendData(player, "There was an communications error. Please try again.");
                                checkMessage = false;
                                receivedDataBuffer = "";
                                //return;
                            }

                            if (eofIndex > 0 && receivedDataBuffer.Length >= eofIndex + 4)// receivedStringBuffer.IndexOf("<EOF>") > 0)//if we've gotten any data from the client pipe at all
                            {
                                //int stringLength = receivedStringBuffer.IndexOf("<EOF>") + 5;
                                string firstMessage = receivedDataBuffer.Substring(0, eofIndex + 5);// receivedStringBuffer.Substring(0, stringLength);
                                firstMessage = firstMessage.Substring(7);
                                firstMessage = firstMessage.Remove(firstMessage.IndexOf("<EOF>"));
                                receivedDataBuffer = receivedDataBuffer.Remove(0, eofIndex + 5);// receivedStringBuffer.Remove(0, stringLength);
                                //Console.WriteLine(firstMessage);
                                //eofIndex = 0;
                               // ConsoleCommands.DoConsoleCommand(firstMessage);
                            }
                            else
                            {
                                checkMessage = false;
                            }
                        } else
                        {
                            checkMessage = false;
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                //not sure if this is for the StreamReader or the read


            }

            //once readFromPipe=false, we need to make sure pipe client is closed, then start a new WaitForPipeClient task in the server queue for the next? client pipe


        }*/

 /*       public void SendToConsole(string message)
        {
            SendToConsole("100", message);

        }

        public void SendToConsole(string msgCode, string message)
        {
            int tempIndex = 0;
            string indexString = tempIndex.ToString("D5");

            string msgString = indexString + "::" + msgCode + "::" + message + "<EOF>";

            tempIndex = msgString.IndexOf("<EOF>");
            indexString = tempIndex.ToString("D5");

            msgString = indexString + "::" + msgCode + "::" + message + "<EOF>";
            //message = message + "<EOF>";

            byte[] msg = Encoding.ASCII.GetBytes(msgString);
            //     if (Program.pipeStream != null)
            //     {
            //Program.pipeStream.Write(msg);
            //Console.WriteLine(msgString);
            if (isOutputConnected)
            {
                namedPipeServer.Write(msg);
                //Program.pipeStream.Flush();
                namedPipeServer.Flush();
            }
        }*/

        public async Task ServerRunning()
        {
            //Console.WriteLine(taskQueue.Count);
            while (taskQueue.Count > 0)
            {
                Task task;
                if (taskQueue.TryDequeue(out task))
                {
                    task.Wait();
                }
                await Task.Delay(1);//not sure if this is needed or not, but just in case so we don't have a wild 'while' running
            }
            //Console.WriteLine("done already?");
        }

        public async Task StartListening()
        {
            listener = TcpListener.Create(Config.config.port);
            listener.Start();
            //isListening = true;

            while (doServerTasks)// isListening)
            {
                try
                {
                    //might need to add tcpclient to different lists, depending.
                    TcpClient incomingClient = await listener.AcceptTcpClientAsync();
                    //Console.WriteLine("blah?");
                    //we need this here so the DoLogin has the player to interact with
                    Player newConnection = new Player();
                    
                    //newConnection.Roundtime = new Timer(newConnection.EndRoundtime, newConnection, -1, -1);
                    //check if guid is used already?
                    //newConnection.Guid = Guid.NewGuid();
                    newConnection.Client = incomingClient;
                    //newConnection.SendData("blah?");
                    newConnection.CurrentServer = this;
                    newConnection.connectionStatus = Player.ConnectionStatus.Connecting;
                    //  newConnection.IP = incomingClient.Client.RemoteEndPoint.ToString();
                    //accountID and accountType are set on the player at successful login down in processMessage
                    //clientTask itself?
                    //Console.WriteLine("blah?");
                    newConnection.SendData("052", $"gameName::{config.gameName}");
                    //ServerFunctions.SendData(newConnection, "052", $"gameName::{Config.config.gameName}");
                    //ServerFunctions.SendData(newConnection, "doh!");

                    //don't wait for this to finish so we're not holding up the line for the next person for however long it takes for this person to get logged in
                    //ServerFunctions.DoLogin(newConnection);

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
                    Log("We have shut down the listener");
                    break;
                //    isListening = false;
                }
                catch (Exception ex)
                {
                    Log($"Blah!? {ex.Message}");
                    break;
                //    isListening = false;
                    //listener.Stop();
                }
            }

        }

        public async Task ServerTicks()
        {



        }

        public async Task RandomOutput()
        {
            Random rnd = new Random();
            //doRandom = true;
            while (doServerTasks)// doRandom)
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
            //checkRTs = true;

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
            while (doServerTasks)// checkRTs)
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
   //             CompileCommands(allScripts, commandDir);
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
  //              CompileCommands(scripts, commandDir);
                //                CompileCommands()
            }
        }
/*
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
                Log($"{success} commands compiled successfully");
                //ServerConsole.newConsole.WriteLine($"{success} commands compiled successfully");
            }
            if (fail > 0)
            {
                Log($"{fail} commands failed to compile");
                //ServerConsole.newConsole.WriteLine($"{fail} commands failed to compile");
            }
            //logFile.WriteLine($"{success} commands compiled successfully");
            //logFile.WriteLine($"{fail} commands failed to compile");
            //ServerConsole.newConsole.WriteLine($"{success} commands compiled successfully");
            //ServerConsole.newConsole.WriteLine($"{fail} commands failed to compile");
        }*/

        public void LoadAdminCommands()
        {
            int loadedCommands = 0;

            Assembly currentAssembly = Assembly.GetCallingAssembly();
            var exportedClasses = currentAssembly.GetExportedTypes().Where(p => typeof(ICommand).IsAssignableFrom(p) && p != typeof(ICommand));

            foreach (var classType in exportedClasses)
            {
             //   if (classType.IsSubclassOf(typeof(ICommand)))
             //   {
                    //Log("blah");
                    var foundClass = (ICommand)Activator.CreateInstance(classType);
                    if (foundClass == null)
                        continue;

                    foreach (string alias in foundClass.Aliases)
                    {
                        Commands newCommand = new Commands();
                        newCommand.commandClass = foundClass;
                        newCommand.commandAlias = alias;
                        //ServerConsole.newConsole.WriteLine(alias + " " + foundClass);
                        adminCommands.Add(newCommand);
                    }
                    loadedCommands++;
             //   }
            }
            if (adminCommands.Count > 0)
            {
                adminCommands = adminCommands.OrderBy(x => x.commandAlias).ToList();
            }

            Log($"{loadedCommands} admin commands loaded!");
            Log($"{adminCommands.Count} total admin command aliases in list!");
            consolePipe.SendToConsole($"{loadedCommands} admin commands loaded!");
            consolePipe.SendToConsole($"{adminCommands.Count} total admin command aliases in list!");
        }

        public void LoadPluginDLL(string serverDirectory)
        {
            int loadedCommands = 0;

            if (Directory.EnumerateFiles(pluginDir).Count() > 0)
            {
                //var allFilenames = Directory.EnumerateFiles(pluginDir).Select(p => Path.GetFileName(p));
                var allDLLFiles = Directory.EnumerateFiles(pluginDir).Select(p => Path.GetFileName(p)).Where(fn => Path.GetExtension(fn) == ".dll").ToArray();
                //Log($"{allDLLFiles.Length}");
                //var commandDLL = allFilenames.Where(fn => Path.GetExtension(fn) == ".dll").ToArray();

                //        String[] commandDLLFiles = Directory.GetFiles(dllDirectory, "*.dll", SearchOption.TopDirectoryOnly);
                //        if (commandDLLFiles.Length == 0)

                for (int i = 0; i < allDLLFiles.Length; i++)
                {
                    //Log($"{allDLLFiles[i]}");
                    var dllFile = Assembly.LoadFile(Path.Combine(pluginDir, allDLLFiles[i]));
                    Type[] dllClasses = dllFile.GetExportedTypes();
                    //Log($"{dllClasses.Length}");
                    foreach (var classType in dllClasses)
                    {
                        if (typeof(ICommand).IsAssignableFrom(classType))
                        {
                            //this class extends from ICommand interface?
                            //Log($"{classType}");
                            var commandClass = (ICommand)Activator.CreateInstance(classType);
                            foreach (string alias in commandClass.Aliases)
                            {
                                Commands newCommand = new Commands();
                                newCommand.commandClass = commandClass;
                                newCommand.commandAlias = alias;
                                commandList.Add(newCommand);
                            }
                            loadedCommands++;
                        }
                    }
                }

     /*           Assembly comAssem = commandAssembly.LoadFromAssemblyPath(Path.Combine(pluginDir, commandDLL[0]));//load the dll file
                Type[] dllClass = comAssem.GetExportedTypes();//get all the public classes from the loaded dll file

                Log($"Command dll for {Config.config.gameName} loaded!");
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
                }*/
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

                Log($"{loadedCommands} commands loaded!");
                Log($"{commandList.Count} total command aliases in list!");
                consolePipe.SendToConsole($"{loadedCommands} commands loaded!");
                consolePipe.SendToConsole($"{commandList.Count} total command aliases in list!");

            } else
            {
                Log("No plugin dlls loaded!");
                consolePipe.SendToConsole("No plugin dlls loaded!");
            }
        }

        public async Task CharacterLogoutTask()
        {
            //doLogouts = true;
            while (doServerTasks)// doLogouts)//while true? maybe some other bool might be better here
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
                            Log($"{connections[i].player.IP} client disconnected.");
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
                    Log($"{player.IP} {player.character.Name} logged out!");// connections[i].account.AccountName} logged out!");
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
                Log("Somebody is connected with an empty guid!");
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
                case "050"://user login from frontend client
                    //find match to splitMessage[1] in login account database here
                    Log(player.IP + " connected!");//Remote, on this socket which is both ends, look at the far end of the connection, from the server point of view
                    //logFile.WriteLine(connections[connectionIndex].clientIP + " connected!");//Remote, on this socket which is both ends, look at the far end of the connection, from the server point of view
                    //Console.WriteLine(client.Client.RemoteEndPoint);
                    ServerFunctions.SendData(player, "<br>Connected! Logging in...", false);// clientGUID, "Connected! Logging in...");//removed serverguid

                    //ServerMain.conn.Open();
                    Account loginAccount = DB.DBAccess.GetLoginAccount(message);// splitMessage[2]);
                    //DataTable loginDT = DBAccess.GetLoginAccount(splitMessage[2]);
                    //Console.WriteLine(loginAccount.HasRows);

                    if (message == " ")
                    {//we sent a " " blank account name. new user?
                        Log(player.IP + " sent a \' \' account name. possible new user?");
                        //logFile.WriteLine(connections[connectionIndex].clientIP + " sent a \' \' account name. possible new user?");
                        //SetClientAccountName(connections[connectionIndex].client, splitMessage[2]);//why would we set the account name to ' ' in the first place?
                        ServerFunctions.SendData(player, "<br>You are trying to login with a blank account name. Either it was an accident or you're a new user. If so, would you like to start a trial account?<br>(If you would like to start a new trial account, please <link=\"clicklink trialstart\">click here</link>, or for more information: <link=\"clicklink trialinfo\">click here</link>)", false);//  type TRIAL START or TRIAL INFO for more information.)");
                        //SendData(clientGUID, "Cannont login: blank account name. Either it was an accident or you're a new user. If so, would you like to start a trial account?<br>(If you would like to start a new trial account, please type /trial start or /trial info for more information.)<br>");
                        return;
                    }
                    if (loginAccount.AccountName == "moo")//default name, obviously new account
                    {//unknown account name. also possible new user?
                        Log(player.IP + " " + message + " unknown account name!");
                        //SetClientAccountName(connections[connectionIndex].client, splitMessage[2]);// somehow get the account name they sent to the new trial bit?
                        ServerFunctions.SendData(player, "<br>Unknown account. Either disconnect and check for typos or would you like to start a trial account?<br>(If you would like to start a new trial account, please <link=\"clicklink trialstart\">click here</link>, or for more information: <link=\"clicklink trialinfo\">click here</link>)\"", false); // type TRIAL START or TRIAL INFO for more information.)");
                        return;
                    }
                    if (message.Length < 8)
                    {//invalid account name
                        Log(player.IP + " " + message + " invalid account name!");
                        ServerFunctions.SendData(player, "<br>Cannot login: invalid account name. How did you even accomplish that?", false);
                        return;
                    }
                    if (!Config.Verify(splitMessage[3], loginAccount.HashedPassword))// loginDT.Rows[0]["hashedPassword"].ToString()))
                    {//password doesn't match
                        Log(player.IP + " " + message + " incorrect password.");
                        //ServerFunctions.SendData(player, splitMessage[3]);
                        ServerFunctions.SendData(player, "<br>Incorrect password. Please disconnect, check your password for typos and try again.", false);
                        return;
                    }
                    // if (loginAccount != null)// loginDT.Rows.Count > 0)
                    // {
                    //we already did a check for the default name so this is a matched account
                    Log(player.IP + " " + message + " logged in!");
                    ServerFunctions.SendData(player, "052", $"account::{message}");
                    player.AccountType = loginAccount.AccountType;
                    //loginAccount.Id is the account Id, the player.Id is the Id for the character they might choose to play
                    // so we need to set the loginAccount.Id to the player's AccountId, not the player.Id
                    //since the AccountId is what we're checking in DoLogin to see if we've connected with a valid account or not (ie, if they're going through the new account process)
                    player.AccountID = loginAccount.Id;
                    player.SendData("<br>Logged in!", false);
                    player.connectionStatus = Player.ConnectionStatus.CharacterSelect;
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

                case "100"://command sent from user to server
                    //this gets added to queue
                    Log($"{player.IP}: \"{message}\"");
                    ServerFunctions.AddCommandToQueue(player, message);
                    break;

                case "105":
                    //clicked hyperlinks from front-end
                    string[] clickedLink = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    //let's get the first word/guid from the clicked link
                    switch (clickedLink[0])
                    {
                        case "charselect":
                            if (player.connectionStatus == Player.ConnectionStatus.CharacterSelect)
                            {
                                int selectedChar = 0;
                                List<Character> characterList = DB.DBAccess.GetCharacters(player, Config.config.gameName);

                                if (Int32.TryParse(clickedLink[1], out selectedChar))
                                {
                                    //already created character
                                    if (selectedChar > -1 && selectedChar < characterList.Count)
                                    {
                                        player.character = characterList[selectedChar];
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
                                        player.connectionStatus = Player.ConnectionStatus.Connected;
                                    }


                                } else
                                {
                                    if (clickedLink[1] == "new")
                                    {
                                        //new character
                                        //get the first room from the server info
                                        player.character = new Character();
                                        Room charGen = DB.DBAccess.GetById<Room>(Config.config.FirstRoom, DB.Collections.Room);//  ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                                        ServerFunctions.SendData(player, "moo?");
                                        ServerFunctions.AddCommandToQueue(player, $"move {charGen.Id}");
                                        //ServerFunctions.SendData(player, "no region,or not instanced");
                                        //     player.CurrentRoom = player.CurrentServer.firstRoom;// ServerFunctions.servers[chosenServer].firstRoom;
                                        player.tempScript = new CharManager(player, player);
                                        ServerFunctions.AddCommandToQueue(player, "look");
                                        player.connectionStatus = Player.ConnectionStatus.CharacterCreation;
                                    }

                                }
                                //player.connectionStatus = Player.ConnectionStatus.Connected;
                                return;

                        /*        //try to convert splitLink[2], ie: the select char option to an int
                                if (!Int32.TryParse(clickedLink[1], out selectedChar))
                                {
                                    ServerFunctions.SendData(player, "<br>An error occured. Please select a character.", false);
                                    ServerFunctions.ShowCharSelection(player);
                                    return;
                                }
                                //put check to see if we're at max character count here?
                                //to see if 'new character' is a valid option or not

                                if (selectedChar > -1 && selectedChar < characterList.Count)
                                {
                                    // if we got the option clicked (greater than -1) and it's less than the char.count
                                    //since 0-3 is less than count of 4
                                    //load character and all that stuff
                                    //or new character
                                    if (characterList[selectedChar].Id != Guid.Empty)
                                    {
                                        //if this is an already created character
                                        //we already have the Character class for this character loaded in the list so we just need to copy it to the player
                                        //ServerFunctions.UpdateCharOnPlayer(player, characterList[option]);
                                        //testing
                                        //player = (Player)characterList[option];
                                        //ServerFunctions.SendData(player, $"testing: {player.Guid.ToString()},{player.AccountType},{player.Name}");
                                        player.character = characterList[selectedChar];
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
                                        Room charGen = DB.DBAccess.GetById<Room>(Config.config.FirstRoom, DB.Collections.Room);//  ServerFunctions.servers[chosenServer].firstRoom, DB.Collections.Room);
                                        ServerFunctions.SendData(player, "moo?");
                                        ServerFunctions.AddCommandToQueue(player, $"move {charGen.Id}");
                                        //ServerFunctions.SendData(player, "no region,or not instanced");
                                        //     player.CurrentRoom = player.CurrentServer.firstRoom;// ServerFunctions.servers[chosenServer].firstRoom;
                                        player.tempScript = new CharManager(player, player);
                                        ServerFunctions.AddCommandToQueue(player, "look");
                                    }
                                    player.connectionStatus = Player.ConnectionStatus.Connected;
                                    return;

                                }
                                else
                                {
                                    ServerFunctions.SendData(player, "An error occured. Please select a character.");
                                    ServerFunctions.ShowCharSelection(player);
                                    //return;
                                }*/
                            }
                            break;

                    }

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
                       /*     case "charselect":
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
                                break;*/

                            case "trialstart":
                                ServerFunctions.SendData(player, "Thank you for wanting to give this game a try! In order to get your trial account set up, please set your account name and password in the provided window.");
                                ServerFunctions.SendData(player, "052", "newaccount::open");
                                ServerFunctions.SendData(player, "052", "trialconfirm::close");
                                player.connectionStatus = Player.ConnectionStatus.NewTrialSetup;
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
                                    player.connectionStatus = Player.ConnectionStatus.CharacterSelect;
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

                    break;

                case "10000"://user forcing a disconnect with their 'disconnect' button, not the same as doing a /logout
                    //logFile.WriteLine("client requesting to logout");
                    ServerFunctions.SendData(player, "10000", "<br>Warning! Forcing a disconnect from the game will not guarantee that your character will be logged out immediately.");//removed serverguid
                    //close this client
                    player.Client.Close();
                    //Thread.Sleep(1000);
                    break;
                default:
                    Log($"{player.IP} unknown message code: {code}");
                    break;
            }
            //Console.WriteLine("message processed?");
        }

        public async Task CheckQueueForCommands()
        {
            //ServerConsole.newConsole.WriteLine("command queue task is started?");
            //checkQueue = true;
            while (doServerTasks)// checkQueue)
            {
                if (CommandsInQueue())// && serverIsRunning)
                {
                    DoCommand(commandQueue[0].player, commandQueue[0].commandMessage);
                    commandQueue.RemoveAt(0);
                }
                await Task.Delay(1);//even this smallest wait will slow down the while loop, hopefully
            }
        }

        public void StopServer()
        {
            //turn off the task bools first so any loops in tasks don't keep going
            doServerTasks = false;
            //serverIsRunning = false;
            waitForPipeClient = false;
            readFromPipe = false;

            try
            {
                //ServerConsole.newConsole.WriteLine("login listener stopped?");
                listener.Stop();
                listenerTask.Wait();
                //Log("listenertask is done");//exceptions from shutting down listener will send to log
            }
            catch (SocketException)
            {
                //newConsole.WriteLine("maybe here?");
                //there were people trying to connect when we closed the listener
            }

            Log($"Disconnecting {connections.Count} players...");
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                //ServerConsole.newConsole.WriteLine(connections[i].player.AccountID);
                //logFile.WriteLine(connections[i].player.IP + " disconnected?");
                ServerFunctions.SendData(connections[i].player, "10000", "You have been booted from the server due to the server shutting down. Please reconnect later. Thank you.");//  connections[i].client, "10000", "goodbye");//removed serverguid
                connections.RemoveAt(i);
            }
            //serverIsRunning = false;
            //doLogouts = false;
            //checkRTs = false;
            //doRandom = false;
            //waitForPipeClient = false;
            //readFromPipe = false;
            //isListening = false;
            logoutTask.Wait();
            roundtimeCheck.Wait();
            //randomOutput.Wait();
            //logFile.WriteLine($"logout task done");
            connections.Clear();
            commandList.Clear();
            //newTrialAccounts.Clear();
            commandQueue.Clear();
            serverIsRunning = false;
            //           commandAssembly.Unload();
            //           commandAssembly = null;
            Log("Server has been shut down.");
            logFile.Close();
            //serverIsRunning = false;
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
