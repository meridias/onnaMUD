using LiteDB;
//using onnaMUD.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using onnaMUD.Utilities;
using onnaMUD.MUDServer;
using onnaMUD.BaseClasses;
using Newtonsoft.Json;
using System.Diagnostics;
using static onnaMUD.MUDServer.ServerMain;

namespace onnaMUD.Characters
{
    public class Player
    {//this is for normal player characters
        //since (most likely) everything in there is session specific, we're not gonna be storing this in the database and so we don't need to worry about [BsonIgnore]
        //public Guid AccountID { get; set; }//so we can see what account has this character, if 0 then nobody?
        //public string Server { get; set; }//which server this character is made on
        //public Character character;
        //public Guid AccountID { get; set; }//account id for the player who has this character

        //since we figured out we can cast (Character)player to a new Character in order to get the Character variables into a new character variable for sending
        //as a json string, we don't need all the [JsonIgnore]s anymore?

        //[JsonIgnore]
        public AccountType AccountType { get; set; }//set this when logging in on this character from the account so we don't have to keep checking the account for the type
        public Guid AccountID { get; set; }
        //[JsonIgnore]
        public TcpClient? Client { get; set; }//so we can get the tcpclient directly from this player class and not have to look anything up after we set it at character login
        //[JsonIgnore]
        public Task? ClientTask { get; set; }//the task for incoming data from client
        //[JsonIgnore]
        public Guid Guid { get; set; } = Guid.Empty;//this will be set at connection, if bad login, will be .Empty
        //[JsonIgnore]
        public string IP { get; set; } = "";//stored string of tcpclient incoming ip address so we can show it after client is disconnected for whatever reason
        //[JsonIgnore]
        public ServerMain? CurrentServer { get; set; } = null;

        public Character? character { get; set; } = null;
        //[JsonIgnore]
        //public Stopwatch RTWatch { get; set; } = new Stopwatch();
        //[JsonIgnore]
        //public int Roundtime { get; set; }//in milliseconds

        //[JsonIgnore]
        //public bool InRT { get; set; } = false;//are we currently in roundtime?
        //[JsonIgnore]
        //public Timer? Roundtime { get; set; }
        //[JsonIgnore]
        public Guid RegionInstance { get; set; }//are we currently in a region instance?
        //[JsonIgnore]
        public dynamic? tempScript { get; set; }

        private ConnectionStatus playerConnStatus = ConnectionStatus.NotConnected;

        //   public void EndRoundtime(Object state)
        // {
        //   InRT = false;
        // ServerFunctions.SendData((Player)state, ">");
        //}
        public ConnectionStatus connectionStatus
        {
            get { return playerConnStatus; }
            set
            {
                playerConnStatus = this.UpdateConnectionStatus(value);
            }
        }

        public enum ConnectionStatus
        {
            NotConnected = 0,
            Connecting = 1,
            CheckingAccount = 2,
            NewTrialSetup = 3,
            CharacterSelect = 4,
            CharacterCreation = 5,
            Connected = 6
        }
    }

}
