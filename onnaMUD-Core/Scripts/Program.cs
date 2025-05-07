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
using System.IO;
using System.Collections.Concurrent;

namespace onnaMUD
{

    public class Program
    {

        //static public string[]? clSplitArgs;
        //static public SQLiteConnection conn;
        //static public List<Server> servers = new List<Server>();

//        static public string consoleDir = "";
//        static private TextWriter? mainErrorLog;
//        static public StreamWriter? newConsole;
        //static public bool checkQueue = false;
        static private List<Task> tasks = new List<Task>();
        //        static private Task? commandTask;
        //        static private Task? consoleTask;
        //static public Config config = new Config();
        static string exeFilename = "";
        //static string pipeName = "onnaMUDPipe";
        //static TaskCompletionSource<bool> isOutputConnected = new TaskCompletionSource<bool>(false);
        static Task? programTask;
        //static Task? pipeRead = null;
        //static public Stream? pipeStream = null;
        //static private bool readFromPipe = false;
        //static ConsoleOutput? output = null;

        private static ConcurrentQueue<Task> processQueue = new ConcurrentQueue<Task>();
        //public static ProcessPrimary primaryClass = null!;
        public static ServerMain server = null!;
        //public static ConsoleOutput console = null!;// = new ConsoleOutput(processPipe);
        public static ProcessPipe processPipe = null!;
        //private static StreamWriter? streamError = null;
        //private static bool isServer = false;

        static async Task Main(string[] args)
        {
            if (!Config.LoadConfig())
            {
                Console.ReadKey();
                //await Task.Delay(3000);
                return;//if we had to create a new appsettings.json file, exit so user can edit file as needed
            }

            //bool isServerStartedAlready = await output.ConnectToPipeServer();

            //bool isConsole = false;//this means that this process is going to be the server console
            //bool isServerStart = false;
            //    string[] allArgs = Environment.GetCommandLineArgs();//this gets all command line args, including the executed filename
            exeFilename = Process.GetCurrentProcess().MainModule.FileName;// allArgs[0];//index 0 is the filename of the executed program
            //Console.WriteLine(args.Length);
            if (args.Length > 0)
            {
                //Console.WriteLine(args[0]);
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
             /*           case "-console"://NO
                            //StartConsoleProcess();
                            //isServer = false;
                            //primaryClass = new ProcessPipe(false);
                            processPipe = new ProcessPipe(false);
                            processPipe.ConnectToPipeServer();
                            console = new ConsoleOutput();
                            //Console.WriteLine(console != null);
                            //await Task.Delay(1000);
                            //output = new ConsoleOutput(true);
                            //isConsole = true;
                            break;
                        case "-startconsole":
                            //processPipe = new ProcessPipe(false);
                            //console = new ConsoleOutput();
                            break;*/
                 //       case "-server":
                 //           StartServerProcess();
                 //           break;
                        case "-startserver":
                            //isServer = true;
                            processPipe = new ProcessPipe();
                            //processPipe = new ProcessPipe(true);
                            //need to check somehow if pipeServer was created or not, if not then skip making a new server and exit
                            
                            if (!processPipe.isPipeServerStarted)
                            {
                                return;
                            }

                            //bool serverCreated = await CreateServer();//make the serverMain
                            AppDomain currentDomain = AppDomain.CurrentDomain;
                            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyExceptionHandler);
                            server = new ServerMain(processPipe);
                            //ServerFunctions.server = new ServerMain(processPipe);
                            //return ServerFunctions.server.isServerCreated;

                            if (server.isServerCreated)//this means serverMain was created and StartServer from that was good
                            {
                                //Console.SetError(ServerFunctions.server.logFile);
                                await Task.Delay(1000);//wait 1 second to let any named pipe outputs connect
                                await server.StartServer();//start the server task
                                AddTaskToQueue(server.ServerRunning());//do the queue.wait thing until we shut everything down
                            //    programTask = ServerFunctions.server.ServerRunning();
                            /*    try
                                {
                                    programTask.Wait();
                                    //return;
                                }
                                catch (Exception ex)
                                {
                                    if (ServerFunctions.server != null)
                                    {
                                        ServerFunctions.server.Log(ex.ToString());
                                        ServerFunctions.server.SendToConsole(ex.ToString());
                                    }
                                    //return;
                                    //Console.WriteLine(ex.ToString());
                                    //Console.ReadKey();
                                }*/
                            }
                            else
                            {
                                return;
                            }
                            //isServerStart = true;
                            break;
                        case "-startserverprocess"://no
                            Process newServer = new Process();

                            ProcessStartInfo serverProcessSI = new ProcessStartInfo()
                            {
                                FileName = exeFilename,
                                UseShellExecute = false,
                                //RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true,
                                ArgumentList = { "-startserver" }
                            };
                        //    newServer.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
                            newServer.StartInfo = serverProcessSI;
                            newServer.Start();
                            newServer.BeginErrorReadLine();

                            break;
                    }

                }

            }
            else
            {
                //no args, so we try to start a new server
                //first, check to see if one is running already by trying to create a new pipe server
                processPipe = new ProcessPipe();

                if (processPipe.isPipeServerStarted)
                {
                    //this means that we just started a new pipe server, meaning one wasn't running yet already
                    //so we shut this one down and start the server process
                    processPipe.CloseServerPipe();
                    StartServerProcess();
                    Console.WriteLine("Server has been started.");
                    //Console.WriteLine("If you would like to start the admin console, run the exe again by itself or add '-console'.");
                    //Console.WriteLine("ex: onnaMUD.exe -console");
                    Console.WriteLine($"{Environment.NewLine}This output will self-destruct in 5 seconds...");
                    //processPipe.CloseClientPipe();
                    await Task.Delay(5000);
                } else
                {
                    //failed in making a new one, so that means one is running already
                    Console.WriteLine("There is a server already running.");
                    Console.WriteLine("If you would like to run multiple servers, please make sure each server you're going to run");
                    Console.WriteLine("is running a .exe from its own directory.");
                    Console.WriteLine($"{Environment.NewLine}This output will self-destruct in 5 seconds...");
                    //processPipe.CloseClientPipe();
                    await Task.Delay(5000);

                }

                    //processPipe = new ProcessPipe(false);
                    //isServer = false;

                    //go ahead and do the 'try and connect to the pipe server to start with and if not, THEN start a new server process?/
                    //if server is already running and we can connect, then console
                    //if can't connect, try new process and try again?
                    //processPipe.pipeConnectTokenSource = new CancellationTokenSource();//?
/*
                    bool tryToConnect = true;
                bool firstTry = true;

                while (tryToConnect)
                {
                    processPipe.clientPipeTimeout = 3000;
                   // processPipe.isPipeConnected = await processPipe.ConnectToPipeServer();

                    if (processPipe.isPipeConnected)
                    {
                        if (firstTry)
                        {
                            //server already running, so start the console
                            tryToConnect = false;
                            //processPipe.CloseClientPipe();
                            //StartConsoleProcess();
                            processPipe.clientPipeTimeout = 0;
                            
                            //console = new ConsoleOutput();
                            //Console.WriteLine("new console?");
                            //await Task.Delay(2000);
                        } else
                        {
                            //second try so we just started server, not Console
                            tryToConnect = false;
                            Console.WriteLine("Server has been started.");
                            //Console.WriteLine("If you would like to start the admin console, run the exe again by itself or add '-console'.");
                            //Console.WriteLine("ex: onnaMUD.exe -console");
                            Console.WriteLine($"{Environment.NewLine}This output will self-destruct in 5 seconds...");
                            processPipe.CloseClientPipe();
                            await Task.Delay(5000);
                        }
                    } else
                    {
                        if (firstTry)
                        {
                            //we didn't connect, so either error/console already connected, or server not started so we'll try to start one
                            firstTry = false;
                            StartServerProcess();
                            await Task.Delay(500);//wait 1/2 second to give any already running outputs a chance to connect to the server before this one tries
                        } else
                        {
                            //didn't connect on the second try so that means error/another console
                            tryToConnect = false;
                            Console.WriteLine("It seems the server failed to start.");
                            Console.WriteLine("Please check server logs for errors and try again.");
                            //Console.WriteLine("Either the server failed to start or you have already another console connected to the server.");
                            //Console.WriteLine("Please check server logs for errors and/or close any other consoles and try again.");
                            Console.WriteLine("Exiting...");
                            processPipe.CloseClientPipe();
                        }
                    }

                }*/

                //processPipe.isPipeConnected = await processPipe.ConnectToPipeServer(5000);

                    //we're not going to bother starting a ConsoleOutput unless we're actually going full console mode
                    //             output = new ConsoleOutput(false);//don't know if we're starting the full console or not

                    //  Console.WriteLine("Checking if server is already running...");
                    //bool isPipeConnected = await output.ConnectToPipeServer();

                    //if (isPipeConnected)
                    // {
                    //server is started and this process has connected to it
                    //do the task queue wait thing here for either the normal output or console to exit
//                    programTask = output.OutputRunning();// ServerFunctions.server.ServerRunning();
//                try
//                {
//                    programTask.Wait();
//                    return;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.ToString());
//                    return;
//                }

                //Console.WriteLine("any key to exit...");
                //Console.ReadKey();
                // } else
                // {
                //either server didn't start, or another output connected to it instead of this one, so this one needs to exit
                //     Console.WriteLine("Either the server failed to start or you have already another console connected to the server.");
                //     Console.WriteLine("Exiting...");
                //     await Task.Delay(4000);
                //Console.WriteLine("Server not started. Starting it up now...");
                //output.ConnectToPipeServer();
                //await Task.Delay(2);
                //StartServerProcess();

                //wait for server to done starting? by way of a response from server?

                // Console.WriteLine("waiting for server to be done starting...");
                // Console.ReadKey();
                //await output.ConnectToPipeServer();
                //pipeRead = ReadFromPipe(clientPipe);
                // return;
                //}

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

            //when we get to this point, either we have something running in a concurrentQueue (serverMain stuff or ConsoleOutput stuff)
            //or we don't
            //if we do, then we'll sit here until they're done/exited/quit/etc. if not, then we'll exit

            while (processQueue.Count > 0)
            {
                Task task;
                if (processQueue.TryDequeue(out task))
                {
                    task.Wait();
                }
                await Task.Delay(1);//not sure if this is needed or not, but just in case so we don't have a wild 'while' running
            }


            //if we're going to be either server console or temp output, start the pipe client. else, do the startserverprocess method
            //           if (!isServerStart)
            //           {
            //Console.WriteLine("trying...");
            //               await output.ConnectToPipeServer();
            //Console.WriteLine("moo");
            //output.SendFromConsole("start server");

            //            }
            //            else
            //            {
            //Console.WriteLine("starting...");
            //we also need to check here is they are trying to run -startserver twice?

            /*         bool serverCreated = await StartServer();
                     if (serverTask != null)
                     {
                         try
                         {
                             serverTask.Wait();
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine(ex.ToString());
                             Console.ReadKey();
                         }
                     }*/

            //           }
            //Console.ReadLine();
            //return;
            //            AppDomain currentDomain = AppDomain.CurrentDomain;
            //            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyExceptionHandler);
            //Start.instance.TempTry();
            //Start blah = new Start();
            //blah.TempTry();

            //           consoleDir = Path.Combine(Directory.GetCurrentDirectory(), "console");
            //           if (!Directory.Exists(consoleDir))
            //           {
            //               Directory.CreateDirectory(consoleDir);
            //           }

            //string logFileName = Config.instance.GetNextLogFileName(consoleDir, "consoleError");
            //           mainErrorLog = new TimeStampedTextWriter(Config.GetNextLogFileName(consoleDir, "consoleError"));//  new TimeStampedStream(Config.instance.GetNextLogFileName(consoleDir, "consoleError"));
            //mainErrorLog = Config.instance.GetNextLogFileName(consoleDir, "consoleError");

            //standard Console.Write/WriteLine redirected to mainErrorLog file to catch any errors thrown by the program (hopefully)
            //string mainLogPath = Path.Combine(Directory.GetCurrentDirectory(), "consoleError.log");
            //mainErrorLog = File.CreateText(mainLogPath);
            //mainErrorLog.AutoFlush = true;

            //            tasks.Add(CheckQueueForCommands());
            //ServerConsole.newConsole.WriteLine("moo?");

            //            tasks.Add(consoleWindow.ConsoleWindow());//the Console.ReadLine blocks? so this needs to be last?
            //ServerConsole consoleWindow = new ServerConsole();
            //tasks.Add(consoleWindow.ConsoleWindow());
            //      tasks.Add(CheckQueueForCommands());
            //ServerConsole.newConsole.WriteLine("moo?");
            //checkQueue = true;
            //commandTask = CheckQueueForCommands();

            //      await Task.WhenAll(tasks);

            //            Console.WriteLine("Server has shut down?");
            //           Console.ReadKey();

            //Console.WriteLine(serversList.Count);
            //           Console.ReadKey();
            //return;

        }

        static void MyExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception excep = (Exception)args.ExceptionObject;
            //Console.WriteLine("MyHandler caught : " + e.Message);
            Console.Error.WriteLine($"***error***{excep.Message}");
            Console.Error.WriteLine($"***error***{excep.StackTrace}");
            if (server != null)
            {
                server.consolePipe.SendToConsole($"***error***{excep.Message}");
                server.consolePipe.SendToConsole($"***error***{excep.StackTrace}");
            }
        }

        static public void StartServerProcess()
        {
            //this should start a new parent process, it does

            ProcessStartInfo serverProcessSI = new ProcessStartInfo()
            {
                FileName = exeFilename,
                UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                CreateNoWindow = true,
                ArgumentList = { "-startserver" }//this new process will be running the actual server
            };
            //serverProcessSI.UseShellExecute = false;
            //serverProcessSI.CreateNoWindow = false;
            //Console.WriteLine(Process.GetCurrentProcess().MainModule.FileName);
            //newServer.StartInfo = serverProcessSI;
            Process.Start(serverProcessSI);
            //newServer = Process.Start(serverProcessSI);
            //newServer.Start();
            //newServer.BeginErrorReadLine();
            //newServer.WaitForExit();
        }

        static public void StartConsoleProcess()
        {
            ProcessStartInfo consoleProcessSI = new ProcessStartInfo()
            {
                FileName = exeFilename,
                UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                RedirectStandardInput = true,
                //CreateNoWindow = true,
                ArgumentList = { "-startconsole" }//this new process will be running the actual server
            };
            //serverProcessSI.UseShellExecute = false;
            //serverProcessSI.CreateNoWindow = false;
            //Console.WriteLine(Process.GetCurrentProcess().MainModule.FileName);
            //newServer.StartInfo = serverProcessSI;
            Process.Start(consoleProcessSI);

        }

 /*       static void ErrorHandler(object sender, DataReceivedEventArgs error)
        {
            if (!String.IsNullOrEmpty(error.Data))
            {
                string todayDate = DateTime.Today.ToString("MM-dd-yyyy");
                string logFileDate = $"crash log {todayDate}";
                bool foundFileName = false;
                int logfileIndex = 1;
                //if there is actually anything in the error
                string crashDir = Path.Combine(Directory.GetCurrentDirectory(), "crash logs");
                if (!Directory.Exists(crashDir))
                {
                    //subdirectory for log files
                    Directory.CreateDirectory(crashDir);
                }

                if (streamError == null)
                {
                    try
                    {
                        string logFileName = $"{logFileDate}.log";// ({logfileIndex}).log";
                        string fullLogPath = Path.Combine(crashDir, logFileName);
                        streamError = new StreamWriter(fullLogPath, true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not open error file!");
                        Console.WriteLine(e.Message.ToString());
                    }
                }

                streamError.WriteLine(String.Format("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), error.Data));  //$"***ERROR*** {error.Data}");
                streamError.Flush();
            }

         //   if (ServerFunctions.server != null)
         //   {
        // Console.WriteLine($"***ERROR*** {error.Data}");
            //Console.Error.WriteLine($"***ERROR*** {e.Data}");
            //Console.Error.Flush();
                //ServerFunctions.server.logFile.WriteLine($"***ERROR*** {e.Data}");
                //ServerFunctions.server.logFile.Flush();
                //Console.Error.WriteLine($"***ERROR*** {e.Data}");
         //   }
            //Console.WriteLine($"***ERROR*** {e.Data}");
        }*/

 /*       static public async Task<bool> CreateServer()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyExceptionHandler);

            ServerFunctions.server = new ServerMain();
            //Console.WriteLine(ServerFunctions.server.isServerCreated);

            return ServerFunctions.server.isServerCreated;
            if (ServerFunctions.server.isServerCreated)
            {
                programTask = ServerFunctions.server.ServerRunning();
                if (programTask != null)
                {
                    Console.WriteLine("good");
                } else
                {
                    Console.WriteLine("bad");
                }
                return true;
            } else
            {
                //something happened and the serverMain didn't get created correctly?
                //or server is already started
                return false;
            }

            //await output.ConnectToPipeServer();
            //wait for console or temp output to connect, then continue
            //await ServerFunctions.server.namedPipeServer.WaitForConnectionAsync();
            //pipeStream = ServerFunctions.server.namedPipeServer;
            //     pipeRead = ReadFromPipe(ServerFunctions.server.namedPipeServer);
            //await isOutputConnected.Task;
            //await ServerFunctions.server.isOutputConnected.Task;
            //       if (!Config.LoadConfig())
            //           return;//if we had to create a new appsettings.json file, exit so user can edit file as needed

           // serverTask = ServerFunctions.server.ServerRunning();//  ServerFunctions.server.StartServer();//we'll do this from a pipe client command
                                                                //         if (serverTask == null)
                                                                //         {
                                                                //if the return task is null, that means the config wasn't loaded and is default so we exit so user can edit
                                                                //             Console.WriteLine("exiting...");
                                                                //             await Task.Delay(3000);
                                                                //         }

            //Console.ReadLine();
            return true;
        }*/

        public static void AddTaskToQueue(Task taskToQueue)
        {
            processQueue.Enqueue(taskToQueue);
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

   /*     static public async Task CheckQueueForCommands()
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
                await Task.Delay(1);//even this smallest wait will slow down the while loop, hopefully
            }
        }*/
        
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
