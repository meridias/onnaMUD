using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using onnaMUD.MUDServer;

namespace onnaMUD.Characters
{
    public static class PlayerExtensions
    {

        public static Player.ConnectionStatus UpdateConnectionStatus(this Player player, Player.ConnectionStatus newStatus)
        {
            switch (newStatus)
            {
                case Player.ConnectionStatus.NotConnected:
                    //if we get set to this by way of this method, then the player was connected at some point and is now not connected so we do a disconnect/logout here

                    break;
                case Player.ConnectionStatus.Connecting:
                    player.Guid = Guid.NewGuid();
                    //newConnection.Client = incomingClient;
                    player.IP = player.Client.Client.RemoteEndPoint.ToString();//  incomingClient.Client.RemoteEndPoint.ToString();
                    player.ClientTask = ConnectedClient(player);//this is the task that checks for incoming data from the frontend

                    ServerMain.Connections tempConn = new ServerMain.Connections();
                    tempConn.player = player;
                    player.CurrentServer.connections.Add(tempConn);
                    //send the created session guid to the player
                    player.SendData("052", $"guid::{player.Guid.ToString()}");
                    break;
                case Player.ConnectionStatus.CheckingAccount:

                    break;

            }



            return newStatus;
        }

        private static async Task ConnectedClient(Player player)// TcpClient client, Player player)// Guid clientGUID)//this is for connecting clients on either login or game servers, any connected client
        {
            //string[] charsToEscape;
            //Console.WriteLine("Client accepted!");
            int numOfBytes = 1024;
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
                            ServerFunctions.SendToLogfile(player, $"{player.AccountID} has an invalid message index. Clearing data buffer.");
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
                            checkMessage = false;
                            receivedDataBuffer = "";
                            //return;
                        }

                        if (eofIndex > 0 && receivedDataBuffer.Length >= eofIndex + 4)//if we got the index correctly and we've got the whole message
                        {
                            string firstMessage = receivedDataBuffer.Substring(0, eofIndex + 5);
                            firstMessage = firstMessage.Substring(7);//this basically removes the EOF index and first :: from the beginning of the message as we don't need it anymore
                            firstMessage = firstMessage.Remove(firstMessage.IndexOf("<EOF>"));
                            receivedDataBuffer = receivedDataBuffer.Remove(0, eofIndex + 5);

                            //sends the message to the ProcessMessage here on ServerFunctions, which finds the server the player is on
                            ServerFunctions.ProcessMessage(player, firstMessage);
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
                    }
                }
            }
        }

    }
}
