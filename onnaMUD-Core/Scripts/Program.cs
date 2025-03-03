using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Specialized;
using System.ComponentModel;
using onnaMUD.MUDServer;
using onnaMUD.Settings;
using onnaMUD.Utilities;
using onnaMUD.Database;
using System.IO.Pipes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using onnaMUD.Characters;

namespace onnaMUD
{

    public class Program
    {

        //static public string[]? clSplitArgs;
        //static public SQLiteConnection conn;
        //static public List<Server> servers = new List<Server>();

        static public string consoleDir = "";
        static private TextWriter? mainErrorLog;
//        static public StreamWriter? newConsole;
        static public bool checkQueue = false;
        static private List<Task> tasks = new List<Task>();
        //        static private Task? commandTask;
        //        static private Task? consoleTask;
        //static public Config config = new Config();
        static string exeFilename = "";
        //static string pipeName = "onnaMUDPipe";
        //static TaskCompletionSource<bool> isOutputConnected = new TaskCompletionSource<bool>(false);
        static Task? serverTask;
        //static Task? pipeRead = null;
        //static public Stream? pipeStream = null;
        //static private bool readFromPipe = false;
        static ConsoleOutput? output = null;

        static async Task Main(string[] args)
        {
            bool isConsole = false;//this means that this process is going to be the server console
            bool isServerStart = false;
            //    string[] allArgs = Environment.GetCommandLineArgs();//this gets all command line args, including the executed filename
            exeFilename = Process.GetCurrentProcess().MainModule.FileName;// allArgs[0];//index 0 is the filename of the executed program

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case "-console":
                            isConsole = true;
                            break;
                        case "-startserver":
                            isServerStart = true;
                            break;
                    }

                }

            } else
            {
                //check to see if server is already running
                //if it is, this process is going to be server console
                //if not, this process is going to be (temporarily) the server startup console output, after we start another process for the server
                //then this process will terminate

                //no args means at least either console or output
                //NamedPipeClientStream clientPipe = new NamedPipeClientStream(".", Config.pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                //pipeStream = clientPipe;
                //               pipeRead = ReadFromPipe(clientPipe);
                //StartServerProcess();
                //see if client is already started
                output = new ConsoleOutput();
                
                Console.WriteLine("Checking if server is already running...");
                bool isPipeConnected = await output.ConnectToPipeServer();

                if (isPipeConnected)
                {
                    Console.WriteLine("Server is running. Starting up server console...");
                    //pipeRead = ReadFromPipe(clientPipe);
                } else
                {
                    Console.WriteLine("Server not started. Starting it up now...");
                    StartServerProcess();
                    //await output.ConnectToPipeServer();
                    //pipeRead = ReadFromPipe(clientPipe);
                }

/*                try
                {
                    Console.WriteLine("Checking if server is already running...");
                    await clientPipe.ConnectAsync(5000);
                    Console.WriteLine("Server is running. Starting up server console...");
                    pipeRead = ReadFromPipe(clientPipe);
                    isConsole = true;
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Server not started. Starting it up now...");
                    StartServerProcess();
                    pipeRead = ReadFromPipe(clientPipe);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await Task.Delay(5000);
                    return;
                }*/

                //await Task.Delay(5000);
                //return;
            }

            //if we're going to be either server console or temp output, start the pipe client. else, do the startserverprocess method
            if (!isServerStart)
            {
                Console.WriteLine("trying...");
                await output.ConnectToPipeServer();


            }
            else
            {
                Console.WriteLine("starting...");
                await StartServer();
                if (serverTask != null)
                {
                    serverTask.Wait();
                }
            }


//            AppDomain currentDomain = AppDomain.CurrentDomain;
//            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyExceptionHandler);
            //Start.instance.TempTry();
            //Start blah = new Start();
            //blah.TempTry();

            consoleDir = Path.Combine(Directory.GetCurrentDirectory(), "console");
            if (!Directory.Exists(consoleDir))
            {
                Directory.CreateDirectory(consoleDir);
            }

            //string logFileName = Config.instance.GetNextLogFileName(consoleDir, "consoleError");
            mainErrorLog = new TimeStampedTextWriter(Config.GetNextLogFileName(consoleDir, "consoleError"));//  new TimeStampedStream(Config.instance.GetNextLogFileName(consoleDir, "consoleError"));
            //mainErrorLog = Config.instance.GetNextLogFileName(consoleDir, "consoleError");

            //standard Console.Write/WriteLine redirected to mainErrorLog file to catch any errors thrown by the program (hopefully)
            //string mainLogPath = Path.Combine(Directory.GetCurrentDirectory(), "consoleError.log");
            //mainErrorLog = File.CreateText(mainLogPath);
            //mainErrorLog.AutoFlush = true;

            //hopefully this works?
            Console.SetError(mainErrorLog);
            //Console.SetOut(mainErrorLog);//redirects Console.WriteLine (from error messages and such that I haven't redirected myself)
            ServerConsole consoleWindow = new ServerConsole();
            //Console.Error.WriteLine("testing?");
            //tasks.Add(consoleWindow.ConsoleWindow());
            //newConsole is the output from the main program/menu that we'll show on the console window but not saved in the mainErrorLog file
            //            newConsole = new StreamWriter(Console.OpenStandardOutput());
            //            newConsole.AutoFlush = true;
            //newConsole.WriteLine("Testing");
            //string logFileName = "";
            //            TextWriter blah = new TimeStampedStream(logFileName);
            //   StreamWriter blah1 = () as StreamWriter;
            //Console.WriteLine("blahblahblahblah    hidey ho");
            //newConsole.WriteLine("does this work?");
            //Console.Write("testing........");
            //string menuWindow = "main";
            //Config.instance.LoadConfig();
//            if (!Config.LoadConfig())
//                return;//if we had to create a new appsettings.json file, exit so user can edit file as needed, or wrong number of login servers found

            //tasks.Add(consoleWindow.ConsoleWindow());//this is the 'menu' part of it

 /*           if (args.Length > 0)
            {
                //Console.WriteLine(args.Length);
                //args will be any servers we want to start automatically (from a bat file?)
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].IndexOf("-") == 0)
                    {
                        //if this arg starts with -, remove it
                        args[i] = args[i].Remove(0, 1);
                    }

                    //for (int j = 0; j < Config.instance.config.servers.Count; j++)
                    //{
                    for (int j = 0; j < ServerFunctions.servers.Count; j++)
                    {
                     //   if (args[i] == ServerFunctions.servers[j].serverInfo.Name)
                     //   {
                     //       ServerFunctions.servers[j].StartServer();
                     //   }
                    }

                }
                //after servers are auto-started, show message, wait 2 seconds, then let the serverconsole proceed
                ServerConsole.newConsole.WriteLine("Servers are done auto-starting.");
                await Task.Delay(2000);
            }*/

            //after loading config, and check command line args, start any auto-start servers
 /*           for (int i = 0; i < ServerFunctions.servers.Count; i++)//why does this cause the ConsoleWindow task to block CheckQueue?
            {
                if (ServerFunctions.servers[i].serverInfo.AutoStart)
                {
                    ServerConsole.newConsole.WriteLine($"Starting up {ServerFunctions.servers[i].serverInfo.Name} server...");
          //          ServerFunctions.servers[i].StartServer();
                    ServerConsole.newConsole.WriteLine($"Done.");
                    await Task.Delay(1000);
                }
            }
            ServerConsole.newConsole.WriteLine("Servers are done auto-starting.");
            await Task.Delay(1500);*/
  
//            tasks.Add(CheckQueueForCommands());
            //ServerConsole.newConsole.WriteLine("moo?");

            tasks.Add(consoleWindow.ConsoleWindow());//the Console.ReadLine blocks? so this needs to be last?
            //ServerConsole consoleWindow = new ServerConsole();
            //tasks.Add(consoleWindow.ConsoleWindow());
            tasks.Add(CheckQueueForCommands());
            //ServerConsole.newConsole.WriteLine("moo?");
            //checkQueue = true;
            //commandTask = CheckQueueForCommands();

            await Task.WhenAll(tasks);

 /*           if (Config.config.servers.Count == 0)
            {
                newConsole.WriteLine("You currently somehow have no servers loaded from the appsettings.");
                newConsole.WriteLine("Kinda hard to interact with servers when there aren't any there, yes?");
                newConsole.WriteLine("Exiting...");
                await Task.Delay(2000);
                return;
            }
            newConsole.WriteLine("Welcome to the onnaMUD main console!");
            checkQueue = true;
            commandTask = CheckQueueForCommands();
            Server? tempServer = null;
            while (true)// !IsServerRunning())
            {
                switch (menuWindow)
                {
                    case "main":
                        int numOfServers = servers.Count;// Config.instance.config.servers.Count;
                        //Console.WriteLine("Please make sure you run the initial server environment option before anything else!");
                        for (int i = 0; i < servers.Count; i++)
                        {
                            if (i == 0)
                            {//we're setting the login server to be first so it's index 0
                                newConsole.WriteLine($"{Environment.NewLine}Login server status: {(servers[0].serverIsRunning ? "Up" : "Not started")}");
                            } else
                            {
                                newConsole.WriteLine($"Game server {servers[i].serverName} status: {(servers[i].serverIsRunning ? "Up" : "Not started")}");
                            }

                        }


                        for (int i = 0; i < servers.Count; i++)
                        {
                            newConsole.WriteLine($"{i + 1}) {(servers[i].serverIsRunning ? $"Shutdown {servers[i].serverName} server" : $"Start {servers[i].serverName} server")}");
                        }

                        newConsole.Write(">");
                        int menuOption;
                        //Int32.TryParse(Console.ReadLine(), out menuOption);

                        if (Int32.TryParse(Console.ReadLine(), out menuOption))// menuOption <= numOfServers)
                        {//parse was successful
                            if (menuOption <= numOfServers && menuOption > 0)
                            {//option picked is in the range of options
                                //newConsole.WriteLine("picked!");
                                //await Task.Delay(2000);
                                tempServer = servers[menuOption - 1];


                                //start/stop server method here
                                await StartOrStopServer(tempServer);
                                //newConsole.WriteLine("next!");
                            } else
                            {//option picked is not in the range of options
                                newConsole.WriteLine("Invalid selection! Please enter the number for the option you want.");
                                await Task.Delay(2000);
                            }
                        } else
                        {//parse failed
                            newConsole.WriteLine("Invalid selection! Please enter the number for the option you want.");
                            await Task.Delay(2000);
                        }
                        break;


                }


                //Console.WriteLine("boo");
                //Thread.Sleep(1000);

                //loginServerStatus = (loginServer == null) ? ": Not started" : ": Up";


                //Console.WriteLine(Environment.NewLine);
                //Console.ReadLine();
            }*/

//            Console.WriteLine("Server has shut down?");
 //           Console.ReadKey();

            //Console.WriteLine($"Exit code of ServerConfig was: {configResult}");
            //  Console.WriteLine("Press any key to exit.");
            //  Console.ReadKey();

            //Console.WriteLine(serversList.Count);
            //Console.ReadKey();
            //return;

        }

        static void MyExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception excep = (Exception)args.ExceptionObject;
            //Console.WriteLine("MyHandler caught : " + e.Message);
            Console.Error.WriteLine(excep.Message);

        }

        static public void StartServerProcess()
        {
            ProcessStartInfo serverProcessSI = new ProcessStartInfo()
            {
                FileName = exeFilename,
                UseShellExecute = true,
                //CreateNoWindow = false,
                ArgumentList = { "-startserver" }
            };
            //serverProcessSI.UseShellExecute = false;
            //serverProcessSI.CreateNoWindow = false;
            //Console.WriteLine(Process.GetCurrentProcess().MainModule.FileName);
            Process.Start(serverProcessSI);

        }

        static public async Task StartServer()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyExceptionHandler);

            ServerFunctions.server = new ServerMain();

            //await output.ConnectToPipeServer();
            //wait for console or temp output to connect, then continue
            //await ServerFunctions.server.namedPipeServer.WaitForConnectionAsync();
            //pipeStream = ServerFunctions.server.namedPipeServer;
            //     pipeRead = ReadFromPipe(ServerFunctions.server.namedPipeServer);
            //await isOutputConnected.Task;
            await ServerFunctions.server.isOutputConnected.Task;
            //       if (!Config.LoadConfig())
            //           return;//if we had to create a new appsettings.json file, exit so user can edit file as needed

            serverTask = ServerFunctions.server.StartServer();
            if (serverTask == null)
            {
                //if the return task is null, that means the config wasn't loaded and is default so we exit so user can edit

            }


        }

 /*       static public async Task StartOrStopServer(Server? tempServer)
        {

            if (tempServer == null)
                return;

            if (tempServer.serverIsRunning)
            {//is running, shut down
                newConsole.WriteLine($"Shutting down {tempServer.serverName} server...");
                tempServer.StopServer();
                newConsole.WriteLine($"Done.");
                await Task.Delay(1000);
            } else
            {//is not running, start up
                newConsole.WriteLine($"Starting up {tempServer.serverName} server...");
                tempServer.StartServer();
                newConsole.WriteLine($"Done.");
                await Task.Delay(1000);
            }

        }*/

        static public async Task CheckQueueForCommands()
        {
            //ServerConsole.newConsole.WriteLine("command queue task is started?");
            checkQueue = true;
            while (checkQueue)
            {
                for (int i = 0; i < ServerFunctions.servers.Count; i++)
                {
                    //ServerFunctions.SendData(ServerFunctions.servers[i].commandQueue[0].player, ServerFunctions.servers[i].CommandsInQueue().ToString());
                    if (ServerFunctions.servers[i].CommandsInQueue() && ServerFunctions.servers[i].serverIsRunning)
                    {
                        //ServerFunctions.SendData(ServerFunctions.servers[i].commandQueue[0].player, "doing command?");
                        ServerFunctions.servers[i].DoCommand(ServerFunctions.servers[i].commandQueue[0].player, ServerFunctions.servers[i].commandQueue[0].commandMessage);
                        //ServerFunctions.servers[i].ProcessMessage(ServerFunctions.servers[i].commandQueue[0].player, ServerFunctions.servers[i].commandQueue[0].commandMessage);
                        ServerFunctions.servers[i].commandQueue.RemoveAt(0);
                    }

                }

          /*      if (loginServer.CommandsInQueue() && loginServer.serverIsRunning)
                {
                    //newConsole.WriteLine("command in login queue");
                    loginServer.ProcessMessage(loginServer.commandQueue[0].clientGuid, loginServer.commandQueue[0].commandMessage);
                    loginServer.commandQueue.RemoveAt(0);
                }
                if (gameServers.Count > 0)
                {
                    for (int i = 0; i < gameServers.Count; i++)
                    {
                        if (gameServers[i].CommandsInQueue() && gameServers[i].serverIsRunning)
                        {
                            gameServers[i].ProcessMessage(gameServers[i].commandQueue[0].clientGuid, gameServers[i].commandQueue[0].commandMessage);
                            gameServers[i].commandQueue.RemoveAt(0);
                        }
                    }
                }*/
                await Task.Delay(1);//even this smallest wait will slow down the while loop, hopefully
            }


        }


        //this should be the clients that have been logged in, authenticated, transfered to game server(s) and will be removed after confirmation    ?
        //no longer needed since this will be on each server class itself
        public class Connections
        {
            public Task? clientTask = null;
            public TcpClient? client = null;
            public string serverOrUser = "";
            public Guid? guid = null;//connected server name or user guid goes here

        }

        
        public class ServerInfo//for the list of servers parsed from the config file
        {
            public string serverName = "login";//this is the argument that will be sent on the executable to start a specific server, ie: -login, -live, etc.
            public IPAddress ipAddress = IPAddress.Parse("127.0.0.1");//ipaddress this server will listen on
            public int port;//port to listen on, for user connections
            public int serverPort;//port to listen on, for game servers connecting to login server, not used for game servers
            public string type = "login";//server type: (login, game). only have 1 login server, as many game servers as you want
            public string access = "default";//(default, restricted) default: all new accounts will automatically be given access to this server. restricted: account needs to be manually given access
            //not needed for login server
            public bool debug = false;//just determines if debug mode will be turned on or kept off on server start. can be enabled/disabled by admin commands during runtime
        }
    }
}
