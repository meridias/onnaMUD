using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using onnaMUD.Characters;
using onnaMUD.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static onnaMUD.Program;

namespace onnaMUD.Utilities
{
    public class ConsoleOutput
    {
        //public NamedPipeClientStream clientPipe = new NamedPipeClientStream(".", Config.config.serverName, PipeDirection.InOut, PipeOptions.Asynchronous);
        public ConcurrentQueue<Task> outputQueue = new ConcurrentQueue<Task>();
        //static public Task? clientPipeRead = null;
        //public ProcessPipe serverPipe;

        private TaskCompletionSource<bool> serverHasStarted;// = new TaskCompletionSource<bool>(false);
        private TaskCompletionSource<bool> receivedServerRunning;// = new TaskCompletionSource<bool>(false);
        //private bool isPipeConnected = false;
        //private bool doPipeServerConnectLoop = false;
        //private bool consoleOpen = false;
        private bool readFromPipe = false;
        public bool isConsole = false;
        //public bool serverIsRunning = false;

        private bool isDoingTest = false;
        private string serverStatus = "";
        private string currentView = "main";
        public string updateView
        {
            get { return currentView; }
            set
            {
                string temp = value;
                if (temp == "" || temp == "refresh")
                {
                    Console.WriteLine("refreshblah");
                    temp = currentView;
                }
                Console.WriteLine("blahbity");
                Task.Delay(1000).Wait();
                switch (temp)
                {
                  //  case "refresh":
                  //      Console.WriteLine("blah");
                  //      goto case "main";
                    case "main":
                        if (processPipe.isPipeConnected)
                        {
                            if (processPipe.serverStatus.isServerRunning)
                                serverStatus = "Up";
                            else
                                serverStatus = "Not started";
                        }
                        else
                            serverStatus = "Down";

                      //  if (currentView == "main")
                      //  {
                            Console.Clear();
                            Console.WriteLine($"Welcome to the {Config.config.gameName} {Config.config.serverName} server console.{Environment.NewLine}**********");
                            Console.WriteLine($"Status of server: {serverStatus}{Environment.NewLine}");
                            if (serverStatus == "Up")
                            {
                                Console.WriteLine($"1) Stop {Config.config.serverName} server");
                            }
                            else
                            {
                                Console.WriteLine($"1) Start {Config.config.serverName} server");
                            }
                      //  }
                        currentView = "main";
                        break;
                    case "1":
                        if (currentView == "main")
                        {
                            if (processPipe.isPipeConnected)
                            {
                                //pipe is connected so start or stop method on ServerMain
                                if (serverStatus == "Up")
                                {
                                    //server is up, so shut down
                                    processPipe.SendFromConsole("55", "false");
                                }
                                else
                                {
                                    //server is down, so start up
                                    processPipe.SendFromConsole("55", "true");
                                }
                            }
                            else
                            {
                                //pipe is not connected so starting new server process is only option?
                                StartServerProcess();
                            }
                            goto case "main";
                        }
                        break;
                    case "test":
                        //consoleOutputView = "test";
                        //Console.Clear();
                        Task testing = TestOutput();
                        Console.ReadKey(true);
                        isDoingTest = false;
                        testing.Wait();
                        break;
                    case "exit":
                        if (currentView == "main")
                        {
                            isConsole = false;
                            console = null;
                        }
                        break;
                    default:
                        updateView = "refresh";
                        break;
                }
            }
        }


        public ConsoleOutput()
        {
            //serverPipe = processPipe;
            processPipe.pipeConnectTokenSource = new CancellationTokenSource();//?

            if (!processPipe.isPipeConnected)
            {
                //Program.AddTaskToQueue(processPipe.ConnectToPipeServerLoop());

            }

            //Console.WriteLine("moo?");
            //Task.Delay(3000).Wait();
            AddTaskToQueue(ConsoleIsOpen());

        }

  /*      public ConsoleOutput(bool isStartConsole)//the bool is whether we already know we're starting the full console
        {
            //add the ConsoleIsOpen() task here to outputQueue??
            isConsole = isStartConsole;
    //        CheckForServer().Wait();

            if (!isConsole)
            {//if this isn't the console already, check if we need it to be
                if (serverPipe.isPipeConnected && serverIsRunning)
                {//if the server is connected and running, then this process will turn into the console
                    isConsole = true;
                    outputQueue.Enqueue(ConsoleIsOpen());
                }
                else
                {
                    //server is not connected and/or not running, and this process wasn't started as the console so exit.
                    if (!serverPipe.isPipeConnected)
                    {//not connected
                        Console.WriteLine("Either the server failed to start or you have already another console connected to the server.");
                        Console.WriteLine("Please check server logs for errors and/or close any other consoles and try again.");
                        Console.WriteLine("Exiting...");
                        //await Task.Delay(10000);
                    }
                    else
                    {//connected, but not running yet
                        Console.WriteLine("If you would like to start the admin console, run the exe again by itself or add '-console'.");
                        Console.WriteLine("ex: onnaMUD.exe -console");
                        Console.WriteLine($"{Environment.NewLine}This output will self-destruct in 5 seconds...");
                        Task.Delay(5000).Wait();
                    }
                    //CloseOutputWindow();
                }

            }
            //outputQueue.Enqueue(ConsoleIsOpen());
            //Console.WriteLine("blah");
        }*/

        public async Task ConsoleIsOpen()
        {
            //this is to keep the console running even if there is not pipe connection, not the same as OutputRunning which is to keep that process running
            //main 'console' loop here?
            isConsole = true;
            //string consoleOutputView = "main";
            updateView = "main";

            //           isPipeConnected = await ConnectToPipeServer();
            //           await Task.Delay(100);//wait for a tic to let the server send the 'isRunning' message through
            //consoleOpen = true;

            /*           if (!isConsole)
                       {
                           //Console.WriteLine(isPipeConnected);
                           if (isPipeConnected)
                           {
                               //we connected to server
                              // try
                             //  {
                                   //SendFromConsole("50", "blah");

             //                      receivedServerRunning = new TaskCompletionSource<bool>(false);
             //                  SendFromConsole("50", "blah");
             //                  bool didWeReceiveFromServer = await WaitForTaskOrTimeout(receivedServerRunning, 5000);
                               //Console.WriteLine(didWeReceiveFromServer);
             //                  if (!didWeReceiveFromServer)
             //                  {
             //                      Console.WriteLine("Server timed out...");
             //                      Console.ReadKey();
             //                      return;
             //                  }

                             //      CancellationTokenSource tokenSource = new CancellationTokenSource(5000);//cancel token after 5 seconds

                             //      TaskCompletionSource<bool> serverTimedOut = new TaskCompletionSource<bool>(false);

                             //      using (tokenSource.Token.Register(() => serverTimedOut.TrySetResult(true)))
                             //      {
                             //          await Task.WhenAny(receivedServerRunning.Task, serverTimedOut.Task);
                     //                  if (clientConnect != await Task.WhenAny(clientConnect, receivedServerRunning.Task))
                     //                  {// whenany returns the task that completed first, so if taskSource gets timed out by the tokenSource, then
                     //                      throw new OperationCanceledException(tokenSource.Token);
                     //                  }
                                       //if the client timed out itself
                     //                  if (clientConnect.Exception?.InnerException != null)
                     //                  {
                     //                      throw clientConnect.Exception.InnerException;
                     //                  }
                               //    }
                          //     }
                          //     catch (OperationCanceledException)
                          //     {
                                   //Debug.Log("timed out, by connection or token");
                                   //mainController.ShowOutput("main", "<br>Unable to connect.");
                          //     }
                          //     catch (Exception)// e)
                          //     {
                                   //Debug.Log("Unexpected exception");
                                   //mainController.ShowOutput("main", "<br>Unable to connect.");

                         //      }

                               if (serverIsRunning)
                               {
                                   //if the server is already up and running, use full console
                                   isConsole = true;
                                   //Console.WriteLine("1");
                               }
                               else
                               {
                                   //server hasn't finished starting yet, so we'll just get the startup messages, then exit
                                   //if the user wants the console, they can run the exe again, with or without -console
                                   //Console.WriteLine("2");
                                   await serverHasStarted.Task;
                                   //wait for 'server started up' message
                                   Console.WriteLine("**********");
                                   Console.WriteLine("Now that the server has finished starting up, this process will exit.");
                                   Console.WriteLine("If you would like to start the admin console, run the exe again by itself or add '-console'.");
                                   Console.WriteLine("ex: onnaMUD.exe -console");
                                   Console.WriteLine($"{Environment.NewLine}This output will self-destruct in 5 seconds...");
                                   await Task.Delay(5000);
                                   CloseOutputWindow();
                                   //return;
                               }
                           }
                           else
                           {
                               //could not connect to server
                               Console.WriteLine("Either the server failed to start or you have already another console connected to the server.");
                               Console.WriteLine("Please check server logs for errors and/or close any other consoles and try again.");
                               Console.WriteLine("Exiting...");
                               await Task.Delay(10000);
                               CloseOutputWindow();
                               //return;
                           }
                       }*/
            //Console.WriteLine("moo");
            while (isConsole)
            {
                /*  if (processPipe.isPipeConnected)
                  {
                      if (processPipe.serverStatus.isServerRunning)
                          serverStatus = "Up";
                      else
                          serverStatus = "Not started";
                  }
                  else
                      serverStatus = "Down";

                  switch (consoleOutputView)
                  {
                      case "main":
                          Console.Clear();
                          Console.WriteLine($"Welcome to the {Config.config.gameName} {Config.config.serverName} server console.{Environment.NewLine}**********");
                          Console.WriteLine($"Status of server: {serverStatus}{Environment.NewLine}");
                          if (serverStatus == "Up")
                          {
                              Console.WriteLine($"1) Stop {Config.config.serverName} server");
                          } else
                          {
                              Console.WriteLine($"1) Start {Config.config.serverName} server");
                          }

                          break;

                  }*/
                //Console.WriteLine("moo?");
                updateView = Console.ReadLine().ToLower();
                //string consoleInput = Console.ReadLine();

          /*      switch (consoleInput.ToLower())
                {
                    case "1":
                        //start/stop server
                        if (processPipe.isPipeConnected)
                        {
                            //pipe is connected so start or stop method on ServerMain
                            if (serverStatus == "Up")
                            {
                                //server is up, so shut down
                                processPipe.SendFromConsole("55", "false");
                            }
                            else
                            {
                                //server is down, so start up
                                processPipe.SendFromConsole("55", "true");
                            }
                        }
                        else
                        {
                            //pipe is not connected so starting new server process is only option?
                            StartServerProcess();
                        }
                        break;
                    case "test":
                        //consoleOutputView = "test";
                        //Console.Clear();
                        Task testing = TestOutput();
                        Console.ReadKey(true);
                        isDoingTest = false;
                        testing.Wait();
                        break;
                    case "exit":
                        isConsole = false;
                        break;

                }*/

            }
        }

        public async Task TestOutput()
        {
            isDoingTest = true;
            Random rand = new Random();

            while (isDoingTest)
            {
                int numUsers = rand.Next(20);
                Console.Clear();
                //Console.SetCursorPosition(0, 0);
                //Console.Write(new string(' ', Console.BufferWidth));
                //Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Number of users: {numUsers}");
                Console.WriteLine($"{Environment.NewLine}Press any key to exit...");
                //Console.ReadKey(true);

                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// If true, task completed first. If false, task timed out
        /// </summary>
        /// <param name="waitTask"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<bool> WaitForTaskOrTimeout(TaskCompletionSource<bool> waitTask, int timeout)
        {
            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);//cancel token after 5 seconds
                TaskCompletionSource<bool> serverTimedOut = new TaskCompletionSource<bool>(false);
                using (tokenSource.Token.Register(() => serverTimedOut.TrySetResult(true)))
                {
                    if (waitTask.Task == await Task.WhenAny(waitTask.Task, serverTimedOut.Task))
                    {
                        return true;
                    } else
                    {
                        waitTask.SetCanceled();
                    }

/*                  if (clientConnect != await Task.WhenAny(clientConnect, receivedServerRunning.Task))
                  {// whenany returns the task that completed first, so if taskSource gets timed out by the tokenSource, then
                      throw new OperationCanceledException(tokenSource.Token);
                  }
                  //if the client timed out itself
                  if (clientConnect.Exception?.InnerException != null)
                  {
                      throw clientConnect.Exception.InnerException;
                  }*/
                }
            }
            catch (OperationCanceledException)
            {
                //Debug.Log("timed out, by connection or token");
                //mainController.ShowOutput("main", "<br>Unable to connect.");
            }
            catch (Exception)// e)
            {
                //Debug.Log("Unexpected exception");
                //mainController.ShowOutput("main", "<br>Unable to connect.");

            }
            return false;
        }

        public async Task OutputRunning()
        {
            //Console.WriteLine(taskQueue.Count);
            while (outputQueue.Count > 0)
            {
                Task task;
                if (outputQueue.TryDequeue(out task))
                {
                    task.Wait();
                }

            }
            //Console.WriteLine("done already?");
        }

        public void CloseOutputWindow()
        {
            readFromPipe = false;
            //clientPipe.Dispose();


        }

    }
}
