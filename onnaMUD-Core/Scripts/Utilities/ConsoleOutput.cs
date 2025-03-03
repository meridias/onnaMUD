using onnaMUD.Settings;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.Utilities
{
    public class ConsoleOutput
    {
        static public NamedPipeClientStream clientPipe = new NamedPipeClientStream(".", Config.pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        static public Task? clientPipeRead = null;

        static private bool readFromPipe = false;

        public ConsoleOutput()
        {



        }



        public async Task<bool> ConnectToPipeServer()
        {
            //NamedPipeClientStream clientPipe = new NamedPipeClientStream(".", Config.pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            //pipeStream = clientPipe;
            try
            {
                //               Console.WriteLine("Checking if server is already running...");
                await clientPipe.ConnectAsync(5000);
                clientPipeRead = ReadFromPipe();
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
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await Task.Delay(5000);
                return false;
            }

        }

        public static void SendConsole(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
       //     if (Program.pipeStream != null)
       //     {
                //Program.pipeStream.Write(msg);
            clientPipe.Write(msg);
                //Program.pipeStream.Flush();
            clientPipe.Flush();
       //     }



        }

        static async Task ReadFromPipe()
        {
            StreamReader pipeReading = new StreamReader(clientPipe);
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

    }
}
