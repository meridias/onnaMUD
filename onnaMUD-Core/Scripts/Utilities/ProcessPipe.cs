using Newtonsoft.Json;
using onnaMUD.MUDServer;
using onnaMUD.Settings;
using onnaMUD.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static onnaMUD.Program;
using static onnaMUD.Settings.Config;

namespace onnaMUD
{
    public class ProcessPipe
    {
        //private bool isServerProcess = false;
        private bool readFromPipe = false;
        private NamedPipeServerStream? namedPipeServer = null!;
  //      private NamedPipeClientStream clientPipe = null!;// new NamedPipeClientStream(".", Config.config.serverName, PipeDirection.InOut, PipeOptions.Asynchronous);
        private ConcurrentQueue<Task> pipeQueue = new ConcurrentQueue<Task>();
        public CancellationTokenSource pipeConnectTokenSource = null!;
        private TaskCompletionSource<bool> serverHasStarted;// = new TaskCompletionSource<bool>(false);
        //private TaskCompletionSource<bool> receivedServerRunning;// = new TaskCompletionSource<bool>(false);
        public bool isPipeConnected = false;
        private bool doPipeServerConnectLoop = false;
        public bool waitForPipeClient = false;
        private bool isOutputConnected = false;
        public bool isPipeServerStarted = false;
        private StreamWriter inputStream;
        public int clientPipeTimeout = 0;


        public ServerStatus serverStatus = new ServerStatus();

        public ProcessPipe()
        {
            try
            {
                namedPipeServer = new NamedPipeServerStream(Config.config.serverName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                Program.AddTaskToQueue(WaitForPipeClient());
                isPipeServerStarted = true;
                //pipeQueue.Enqueue(WaitForPipeClient());
                //tasks.Add(WaitForPipeClient());
            }
            catch (IOException)
            {
                //tried to start another server instance
                isPipeServerStarted = false;
                //return;
            }


        }

  /*      public ProcessPipe(bool isServer)
        {
            isServerProcess = isServer;
            if (isServer)
            {
                try
                {
                    namedPipeServer = new NamedPipeServerStream(Config.config.serverName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    Program.AddTaskToQueue(WaitForPipeClient());
                    isPipeServerStarted = true;
                    //pipeQueue.Enqueue(WaitForPipeClient());
                    //tasks.Add(WaitForPipeClient());
                }
                catch (IOException)
                {
                    //tried to start another server instance
                    isPipeServerStarted = false;
                    return;
                }

            } else
            {
                clientPipe = new NamedPipeClientStream(".", Config.config.serverName, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipeConnectTokenSource = new CancellationTokenSource();//?
                //processPipe.pipeConnectTokenSource = new CancellationTokenSource();//?
                //Program.AddTaskToQueue(ConnectToPipeServerLoop());// processPipe.ConnectToPipeServerLoop());
            }
        }*/

 /*       public async Task<bool> ConnectToPipeServer()//int timeout = 0)
        {
            //NamedPipeClientStream clientPipe = new NamedPipeClientStream(".", Config.pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            //pipeStream = clientPipe;
            //     do//will do the connection once, even if it's not a console. if it is, will keep going
            //     {
            try
            {
                //Console.WriteLine("trying...");
                //               Console.WriteLine("Checking if server is already running...");
                if (clientPipeTimeout > 0)
                {
                    //this is for the initial program no-argument check if server is already running
                    //this will return true or false by either connecting or timing out
                    await clientPipe.ConnectAsync(clientPipeTimeout);//, pipeConnectTokenSource.Token);
                    pipeQueue.Enqueue(ReadFromPipe());
                }
                else
                {
                    //this should be for doing a loop for reconnecting to pipe server from console
                    //this we need to actually set isPipeConnected = true since we're not awaiting the bool result on this task
                    await clientPipe.ConnectAsync(pipeConnectTokenSource.Token);
                    isPipeConnected = true;
                    pipeQueue.Enqueue(ReadFromPipe());
                    //isPipeConnected = true;
                    //Console.WriteLine("connected?");
                    //await ReadFromPipe();
                    //isPipeConnected = false;
                }
                //Console.WriteLine("connected?");
                //      isPipeConnected = true;
                //Console.WriteLine("connected?");
                //      await ReadFromPipe();
                //      isPipeConnected = false;
                //pipeQueue.Enqueue(ReadFromPipe());
                //clientPipeRead = ReadFromPipe();
                //               Console.WriteLine("Server is running. Starting up server console...");
                //               pipeRead = ReadFromPipe(clientPipe);
                //isConsole = true;

                return true;
            }
            catch (TimeoutException)
            {
                //                Console.WriteLine("Server not started. Starting it up now...");
                //                StartServerProcess();
                //pipeRead = ReadFromPipe(clientPipe);
                //Console.WriteLine("timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await Task.Delay(5000);
                return false;
            }
            //      }
            //      while (console != null);

        }

        public async Task ConnectToPipeServerLoop()
        {
            doPipeServerConnectLoop = true;
            //pipeConnectTokenSource = new CancellationTokenSource();//?

            while (doPipeServerConnectLoop)
            {
                try
                {
                    //Console.WriteLine("trying...");
                    //               Console.WriteLine("Checking if server is already running...");
                    await clientPipe.ConnectAsync(pipeConnectTokenSource.Token);
                    isPipeConnected = true;
                    //Console.WriteLine("connected?");
                    await ReadFromPipe();
                    isPipeConnected = false;
                    //outputQueue.Enqueue(ReadFromPipe());

                }
                catch (TimeoutException)
                {
                    //                Console.WriteLine("Server not started. Starting it up now...");
                    //                StartServerProcess();
                    //pipeRead = ReadFromPipe(clientPipe);
                    //Console.WriteLine("timed out");
                   // return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await Task.Delay(5000);
                    //return false;
                }


            }


        }*/

        async Task WaitForPipeClient()//server side
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
                    SendStatusToConsole();
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

        }

        public async Task SendStatusToConsole()
        {
            string jsonString = "";
            while (isOutputConnected)
            {
                serverStatus.isServerRunning = server.serverIsRunning;

                jsonString = JsonConvert.SerializeObject(serverStatus);
                SendToConsole("50", jsonString);
                await Task.Delay(1500);//send server status every 1.5 seconds
            }


        }

  /*      public void SendFromConsole(string msgCode, string message)
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
            clientPipe.Write(msg);
            //Program.pipeStream.Flush();
            clientPipe.Flush();
            //     }
        }*/

        public void SendToConsole(string message)
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
        }



        public async Task ReadFromPipe()
        {
            //StreamReader pipeReading = new StreamReader(clientPipe);
            int numOfBytes = 1024;
            string receivedDataBuffer = "";
            byte[] bytes = new byte[numOfBytes];
            bool checkMessage = false;
            int eofIndex = 0;
            readFromPipe = true;
            //Console.WriteLine("moo?");
            while (readFromPipe)
            {
                try
                {
                    int bytesRec = 0;// await clientPipe.ReadAsync(bytes, 0, numOfBytes);
                //    if (isServerProcess)
                        bytesRec = await namedPipeServer.ReadAsync(bytes, 0, numOfBytes);
                  //  else
                    //    bytesRec = await clientPipe.ReadAsync(bytes, 0, numOfBytes);

                    if (bytesRec > 0)
                    {
                        receivedDataBuffer += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        checkMessage = true;
                    }
                    else
                    {
                        readFromPipe = false;
                        checkMessage = false;
                    //    if (isServerProcess)
                            namedPipeServer.Disconnect();
                      //  else
                        //{
                          //  isPipeConnected = false;
                            //if (console != null)
                           // {
                                //if this pipe is console client, re-run the ConnectToPipeServer task to wait for another connection?
                           //     ConnectToPipeServer();
                           // }
                       // }
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
                    if (receivedDataBuffer.IndexOf("::") == 5)
                    {
                        try
                        {
                            eofIndex = Int32.Parse(receivedDataBuffer.Substring(0, 5));//parse the first 5 characters for the index of EOF
                        }
                        catch (FormatException)
                        {
                            checkMessage = false;
                            receivedDataBuffer = "";
                            //return;
                        }

                        if (eofIndex > 0 && receivedDataBuffer.Length >= eofIndex + 4)
                        {
                            string firstMessage = receivedDataBuffer.Substring(0, eofIndex + 5);
                            firstMessage = firstMessage.Substring(7);
                            firstMessage = firstMessage.Remove(firstMessage.IndexOf("<EOF>"));
                            receivedDataBuffer = receivedDataBuffer.Remove(0, eofIndex + 5);
                            ProcessMessage(firstMessage);
                        }
                        else
                        {
                            checkMessage = false;
                        }
                    }
                    else
                    {
                        checkMessage = false;
                    }
                }
            }
        }

        public void CloseClientPipe()
        {
            isPipeConnected = false;
            readFromPipe = false;
 //           clientPipe.Close();


        }

        public void CloseServerPipe()
        {
            waitForPipeClient = false;
            readFromPipe = false;
            namedPipeServer.Dispose();
        }

        public async Task ProcessMessage(string fullMessage)//console side
        {
            string[] delimiter = { "::" };
            string[] splitMessage = fullMessage.Split(delimiter, StringSplitOptions.None);

            //index 0 is message code, will ignore from now on after this point
            //index 1 is message

            string code = splitMessage[0];
            string message = splitMessage[1];

            switch (code)
            {
                case "50":
                    //this is the console receiving the serverStatus from the server
                    //Console.WriteLine(fullMessage);
       //             bool currentServerStatus = serverStatus.isServerRunning;
       //             serverStatus = JsonConvert.DeserializeObject<ServerStatus>(message);
           /*         if (currentServerStatus != serverStatus.isServerRunning)
                    {
                        //?
                        Console.WriteLine("refreshing?");
                        Console.WriteLine(console != null);
                        console.updateView = "refresh";
                        //MemoryStream stringStream = new MemoryStream();
                        //var stringWriter = new StreamReader(stringStream);
                        //StringReader sr = new StringReader($"moo{Environment.NewLine}");
                        
                        //Console.SetIn(sr);
                        //Console.WriteLine("refreshing?");
                        //await Task.Delay(1000);
                        //inputStream = Console.
                        //Console.SetIn(stringWriter);
                        //Console.In.
                        //stringStream.Write("blah")
                        //Console.OpenStandardInput();
                    }*/

         /*           if (isServerProcess)//we got code 50 from the console so we send back if server is running
                    {
                        //server has been requested to let console know if server is running or not
                        SendToConsole("50", server.serverIsRunning.ToString());
                    } else//we got code 50 from the server so now we know if server is running
                    {
                        //has the server started up yet? true or false
                        Boolean.TryParse(message, out console.serverIsRunning);
                        receivedServerRunning.SetResult(true);
                    }*/
                    break;
                case "55":
                    //start/stop server
                    //true means start, false means stop
                    bool startOrStopServer = false;
                    if (Boolean.TryParse(message, out startOrStopServer))//if the parse was successful
                    {
                        if (startOrStopServer)
                        {
                            server.StartServer();
                        } else
                        {
                            server.StopServer();
                        }
                    }
                    break;
                case "60":
                    //this is the signal from the server that it has finished starting up. the actual message doesn't matter
                    serverHasStarted.SetResult(true);
                    break;
                case "100":
                    //basic console text
          //          if (console != null)
            //            Console.WriteLine(message);
                    break;
                case "start server"://?
                    server.StartServer();
                    break;
                default:

                    break;


            }

        }

        public class ServerStatus
        {
            public bool isServerRunning = false;
            public int numOfChars = 0;

        }

    }
}
