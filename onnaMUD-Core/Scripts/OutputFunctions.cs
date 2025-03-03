using onnaMUD.BaseClasses;
using onnaMUD.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.MUDServer
{
    public static class OutputFunctions
    {
        //this are the extensions to output text to their respective front-end windows
        public static void OutputToMain(this Player player, string dataToSend, bool sendCaret = true)
        {
            if (!sendCaret)
            {
                player.SendData("110", dataToSend, false);
                //ServerFunctions.SendData(player, "110", dataToSend, false);
            }
            else
            {
                player.SendData("110", dataToSend);
                //ServerFunctions.SendData(player, "110", dataToSend);
            }
        }

        public static void SendToRoom(Room room, Player player, string playerMessage, string roomMessage)
        {
            for (int i = 0; i < room.StuffInRoom.Count; i++)
            {
                if (room.StuffInRoom[i].ThingType == ThingType.Character)
                {
                    Character charInRoom = (Character)room.StuffInRoom[i];
                    if (charInRoom.Player == null)
                        continue;

                    if (charInRoom.Player == player)
                    {
                        player.OutputToMain(playerMessage);
                    }
                    else
                    {
                        charInRoom.Player.OutputToMain(roomMessage);
                    }
                }
            }
        }

        public static void SendToRoom(Room room, Player player, string playerMessage, Player target, string targetMessage, string roomMessage)
        {
            if (target == null)
            {
                SendToRoom(room, player, playerMessage, roomMessage);
                return;
            }

            for (int i = 0; i < room.StuffInRoom.Count; i++)
            {
                if (room.StuffInRoom[i].ThingType == ThingType.Character)
                {
                    Character charInRoom = (Character)room.StuffInRoom[i];
                    if (charInRoom.Player == null)
                        continue;

                    if (charInRoom.Player == player)
                    {
                        player.OutputToMain(playerMessage);
                    }
                    else if (charInRoom.Player == target)
                    {
                        target.OutputToMain(targetMessage);
                    } else
                    {
                        charInRoom.Player.OutputToMain(roomMessage);
                    }
                }
            }
        }



    }
}
