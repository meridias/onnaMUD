//using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//using static onnaMUD.ServerMain;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using onnaMUD.MUDServer;
using onnaMUD.Database;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
using static onnaMUD.Program;

namespace onnaMUD.Settings
{
    public class Start
    {
        public static Start instance;
        public Start()
        {
            instance = this;
        }
        public bool TempTry()
        {
            return true;
        }

    }

    public class Config
    {

        //public static Config instance = new Config();// = this;
        //static public SQLiteConnection? conn;
        //static private IConfigurationBuilder? configuration;// = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        //static private IConfigurationRoot? config;// = configuration.Build();

        //password hash stuff
        private const int _saltSize = 16;//128 bits in hex form
        private const int _keySize = 32;//256 bits in hex form
        private const int _iterations = 50420;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;
        private const char segmentDelimiter = ':';
        //private string? jsonString = "";

        public static AppSettings config = new AppSettings();
        public static string scriptDir = "";

        public static List<string> baseClasses = new List<string>();
        //static public List<ServerInfo> serversList = new List<ServerInfo>();

        public static List<AccountSettings> accountSettings = new List<AccountSettings>();
        //public static string pipeName = "onnaMUDPipe";

        public Config()
        {
            //instance = this;

        }

        public static bool LoadConfig()
        {
            string? jsonString = "";
            if (!File.Exists("appsettings.json"))
            {
                //ServerFunctions.server.SendToConsole("appsettings.json file could not be found! Creating default file.");
                server.consolePipe.SendToConsole("appsettings.json file could not be found! Creating default file.");
                server.consolePipe.SendToConsole("Please edit this file to your preferred configuration.");
                server.consolePipe.SendToConsole("Press any key to exit...");
                //config.servers.Add(new SettingsServers());
                jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText("appsettings.json", jsonString);
                return false;

            }
            else
            {
                jsonString = File.ReadAllText("appsettings.json");
                config = JsonConvert.DeserializeObject<AppSettings>(jsonString);

                //ServerConsole.newConsole.WriteLine("appsettings.json file loaded!");
  //              ServerFunctions.server.SendToConsole("appsettings.json file loaded!");

                //for now...
                //return true;

 /*               int numOfLogins = 0;

                foreach (SettingsServers server in config.servers)
                {
                    //checking to see if there is more than 1 login server set
                    if (server.type == "login")
                    {
                        numOfLogins++;
                    }
                }

                if (numOfLogins > 1 || numOfLogins == 0)
                {
                    ServerConsole.newConsole.WriteLine("Please make sure that you only have 1 login server set up in appsettings.json.");
                    return false;
                }

                for (int i = 0; i < config.servers.Count; i++)
                {//so the login server gets added first
                    if (config.servers[i].type == "login")
                    {
                        //ServerMain.servers.Add(new Server(config.servers[i].serverName, config.servers[i].type, config.servers[i].debug));
                        ServerFunctions.servers.Add(new ServerMain(config.servers[i].serverName, config.servers[i].type, config.servers[i].debug));
                    }
                }
                for (int i = 0; i < config.servers.Count; i++)
                {
                    if (config.servers[i].type == "game")
                    {
                        //ServerMain.gameServers.Add(new Server(config.servers[i].serverName));
                        ServerFunctions.servers.Add(new ServerMain(config.servers[i].serverName, config.servers[i].type, config.servers[i].debug));
                    }
                }*/
                //Console.WriteLine(ServerMain.loginServer.serverIsRunning);
                //Console.WriteLine(config.servers.Count);
                //return true;
            }

            //gotta see if the db is there before we try to connect to it to do stuff
 //           ServerFunctions.server.SendToConsole("Checking for local database...");
 //           DB.CheckForLocalDB();
            //moving the server info out of the config and putting it in the database
            //get the serverinfo for each created server, if any
            //?
 /*           if (DB.DBAccess.DoesCollectionExist(DB.Collections.Server))
            {
                var servers = DB.DBAccess.GetList<Server>(DB.Collections.Server);
                foreach (Server server in servers)
                {
                    ServerMain temp = new ServerMain();
                    //ServerFunctions.LoadedServer temp = new ServerFunctions.LoadedServer();
                    //temp.serverInfo = server;
                    //temp.serverClass = new ServerMain(server);
                    ServerFunctions.servers.Add(temp);
                }
            }
            ServerFunctions.server.SendToConsole("Local database loaded.");*/
            //load the .cs files from BaseClasses directory so we can read the script sources as strings for object editing...
            //yeah, no. gonna set the strings to send to frontend manually
//            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "BaseClasses")))
//            {
                //ServerConsole.newConsole.WriteLine("class directory found");
//                baseClasses = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "BaseClasses"), "*.cs", SearchOption.TopDirectoryOnly).ToList();

                //var allFilenames = Directory.EnumerateFiles(serverDirectory).Select(p => Path.GetFileName(p));
                //var commandDLL = allFilenames.Where(fn => Path.GetExtension(fn) == ".dll").ToArray();

//            }

            //after we've added the servers to the ServerMain server list
            //scriptDir = Path.Combine(Directory.GetCurrentDirectory(), "command scripts");//not gonna need this either, will put command dll in each server dir
            //       if (!Directory.Exists(scriptDir))
            //     {
            //       Directory.CreateDirectory(scriptDir);
            // }
            //create a command script directory for all servers
            //not gonna do this, compiled dll for server specific* commands (just dll for each server if we want, or just copy the same one to each serve dir)
            //if (!Directory.Exists(Path.Combine(scriptDir, "all")))
            // {
            //   Directory.CreateDirectory(Path.Combine(scriptDir, "all"));
            // }

            //only do this when a server gets started?
            //           for (int i = 0; i < ServerFunctions.servers.Count; i++)
            //           {
            /*   string serverSubPath = Path.Combine(Directory.GetCurrentDirectory(), ServerFunctions.servers[i].serverInfo.Name);// serverName);
               ServerFunctions.servers[i].serverClass.serverSubDir = serverSubPath;
               //make sure the main server directory and logs directory are created
               if (!Directory.Exists(serverSubPath))
               {
                   Directory.CreateDirectory(serverSubPath);
               }
               if (!Directory.Exists(Path.Combine(serverSubPath, "logs")))
               {
                   Directory.CreateDirectory(Path.Combine(serverSubPath, "logs"));
               }*/

            /*      //create a command script directory for each game server
                  if (ServerFunctions.servers[i].serverInfo.Type == "game")// serverType == "game")
                  {
                      if (!Directory.Exists(Path.Combine(scriptDir, ServerFunctions.servers[i].serverInfo.Name)))
                      {
                          Directory.CreateDirectory(Path.Combine(scriptDir, ServerFunctions.servers[i].serverInfo.Name));
                      }
                  }*/

            //for now, putting this in every server including login so we can test if the creating dll thing works
            //if (!Directory.Exists(Path.Combine(serverSubPath, "scripts")))
            //{
            //  Directory.CreateDirectory(Path.Combine(serverSubPath, "scripts"));
            // }
            /*     if (!Directory.Exists(Path.Combine(serverSubPath, "commands")))
                 {
                     Directory.CreateDirectory(Path.Combine(serverSubPath, "commands"));
                 }*/
            //            }

            /*          foreach (SettingsServers server in config.servers)//  IConfigurationSection section in configServers.GetChildren())
                      {
                          string serverSubPath = Path.Combine(Directory.GetCurrentDirectory(), server.serverName);
                          if (!Directory.Exists(serverSubPath))// Path.Combine(serverSubPath, server.serverName)))// section["serverName"])))
                          {
                              Directory.CreateDirectory(serverSubPath);// Path.Combine(serverSubPath, server.serverName));// section["serverName"]));
                          }
                          if (!Directory.Exists(Path.Combine(serverSubPath, "logs")))
                          {
                              Directory.CreateDirectory(Path.Combine(serverSubPath, "logs"));
                          }

                          if (server.type == "login")
                          {
                              ServerMain.loginServer.serverSubDir = serverSubPath;
                          } else
                          {
                              for (int i = 0; i < ServerMain.gameServers.Count; i++)
                              {
                                  if (server.serverName == ServerMain.gameServers[i].serverName)
                                  {
                                      ServerMain.gameServers[i].serverSubDir = serverSubPath;
                                      if (!Directory.Exists(Path.Combine(serverSubPath, "scripts")))
                                      {
                                          Directory.CreateDirectory(Path.Combine(serverSubPath, "scripts"));
                                      }
                                      if (!Directory.Exists(Path.Combine(serverSubPath, "commands")))
                                      {
                                          Directory.CreateDirectory(Path.Combine(serverSubPath, "commands"));
                                      }
                                  }
                              }
                          }

                      }*/
            //DB.CheckForLocalDB();
            //DBAccess.CheckForLocalDB();

            /*          string serverDatabasePath = Directory.GetCurrentDirectory();// Path.Combine(Directory.GetCurrentDirectory(), serverName);
                                                                                  //make the connection string
                      string serverDatabaseFile = Path.Combine(serverDatabasePath, "onnaMUD.db");
                      string connString = $"Data Source={serverDatabaseFile}";// Path.Combine(serverDatabasePath, serverName)}.db";//in case we need to add parameters later...
                                                                              //server.sqliteConn = connString;
                      ServerMain.conn = new SQLiteConnection(connString);

                      if (!File.Exists(serverDatabaseFile))
                      {
                          ServerMain.conn.Open();

                          string accountTable = "CREATE TABLE accounts (accountID INTEGER PRIMARY KEY AUTOINCREMENT, accountName TEXT, hashedPassword TEXT, accountType TEXT)";
                          //Console.WriteLine("Creating accounts table...");
                          SQLiteCommand cmd = new SQLiteCommand(accountTable, ServerMain.conn);//creating accounts table
                          cmd.ExecuteNonQuery();

                          //inserting primary admin account
                          //doing it this way with AddWithValue is apparently supposed to help against sql injection, per: IronPDF.com "c# sqlite (how it works for developer)"
                          //Console.WriteLine("Inserting primary admin account...");
                          string adminAccount = "INSERT INTO accounts (accountName, hashedPassword, accountType) VALUES (@accountName, @password, @type)";
                          cmd = new SQLiteCommand(adminAccount, ServerMain.conn);
                          cmd.Parameters.AddWithValue("@accountName", $"{config.primaryAdminAccount}");//  config["primaryAdminAccount"]}");
                          cmd.Parameters.AddWithValue("@password", Hash(config.auth));//  config["auth"]));
                          cmd.Parameters.AddWithValue("@type", "admin");
                          //Console.WriteLine(cmd.ExecuteNonQuery());
                          cmd.ExecuteNonQuery();

                          string charTable = "CREATE TABLE characters (charID INTEGER PRIMARY KEY AUTOINCREMENT, charName TEXT, accountID INTEGER)";
                          //Console.WriteLine("Creating characters table...");
                          cmd = new SQLiteCommand(charTable, ServerMain.conn);//creating characters table
                          cmd.ExecuteNonQuery();

                          ServerMain.conn.Close();
                      }*/

  /*          ServerFunctions.server.SendToConsole("Setting account values...");
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
            ServerFunctions.server.SendToConsole("Account values set!");*/

            return true;
        }

        public static SettingsServers? GetServerInfo(string serverToStart)
        {
            //jsonString = File.ReadAllText("appsettings.json");
            //config = JsonConvert.DeserializeObject<AppSettings>(jsonString);
            //need to redo this. 
            //ServerInfo serverTemp = new ServerInfo();
            //Console.WriteLine(serverToStart);
            //var configServers = config.GetSection("servers");
     /*       foreach (SettingsServers server in config.servers)// IConfigurationSection section in configServers.GetChildren())
            {
                //Console.WriteLine(server.serverName);
                if (serverToStart.IndexOf(server.serverName) > -1)// section["serverName"]) > -1)
                {
//                    string serverName = server.serverName;// section["serverName"];
//                    string serverDatabasePath = Path.Combine(Directory.GetCurrentDirectory(), serverName);
                    //make the connection string
//                    string serverDatabaseFile = Path.Combine(serverDatabasePath, serverName + ".db");
//                    string connString = $"Data Source={Path.Combine(serverDatabasePath, serverName)}.db";//in case we need to add parameters later...
//                    server.sqliteConn = connString;

                    //if we matched a server from the config to the serverToStart tag
                    return server;

                }
            }*/
            return null;
        }

        public string? CheckServerName(string nameToCheck)
        {
            //var configServers = config. GetSection("servers");
      /*      foreach (SettingsServers server in config.servers)// IConfigurationSection section in configServers.GetChildren())
            {
                if (nameToCheck.IndexOf(server.serverName) > -1)//  section["serverName"]) > -1)
                {
                    if (nameToCheck.IndexOf("-") == 0)
                    {
                        //if the name to check starts with -, remove it
                        nameToCheck = nameToCheck.Remove(0, 1);//this should be just removing the first character
                        return nameToCheck;
                    }
                    else
                    {
                        return server.serverName;// section["serverName"];
                    }
                }
            }*/

            return null;
        }

        public static string ConfigInfo(string infoNeeded)
        {
            string result = "";

            switch (infoNeeded)
            {
                case "gameName":
                    result = config.gameName;// ["gameName"];
                    break;
                case "auth":
                    result = config.auth;// ["auth"];
                    break;
                default:
                    break;


            }
            return result;
        }

        public static string GetNextLogFileName(string directory, string logID)//logID would be the servername or console
        {
            string todayDate = DateTime.Today.ToString("MM-dd-yyyy");
            string logFileDate = $"{logID} {todayDate}";
            bool foundFileName = false;
            int logfileIndex = 1;
            //StreamWriter? tempWriter = null;// new StreamWriter(Path.Combine(directory, logID));//temp log file name
            string logWriterName = "";
            try
            {
                //string logDirectory = Path.Combine(directory, "logs");
                String[] logFiles = Directory.GetFiles(directory, "*.log", SearchOption.TopDirectoryOnly);

                if (logFiles.Length > 0)
                {//if there are logs already in this directory, go through them and look for a match in order to find the next available index
                    while (!foundFileName)
                    {
                        //Console.WriteLine(logfileIndex);//can't use newConsole since it might not be set yet?
                        for (int i = 0; i < logFiles.Length; i++)
                        {
                            if (logFiles[i].Contains($"{logFileDate} ({logfileIndex})"))
                            {
                                foundFileName = true;
                                //logfileIndex++;
                                break;
                            }
                        }
                        if (foundFileName)
                        {
                            //means there was a match, go again
                            foundFileName = false;
                            logfileIndex++;
                        }
                        else
                        {
                         //   if (logfileIndex == logFiles.Length)
                         //   {//if we went through all the files and didn't find a match, add 1 so we're using the next index for this logfile
                         //       logfileIndex++;
                         //   }
                            //means there wasn't a match, use the current index and make the logfile
                            string logFileName = $"{logFileDate} ({logfileIndex}).log";
                            string fullLogPath = Path.Combine(directory, logFileName);
                            logWriterName = fullLogPath;
                            //tempWriter = File.CreateText(fullLogPath);
                            foundFileName = true;
                        }

                    }
                }
                else
                {//no logs in this directory, go ahead and make a new one with index of 1
                    string logFileName = $"{logFileDate} ({logfileIndex}).log";
                    string fullLogPath = Path.Combine(directory, logFileName);
                    logWriterName = fullLogPath;
                    //tempWriter = File.CreateText(fullLogPath);
                }
                //logFile.AutoFlush = true;
                //newConsole.WriteLine("logfile created");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
            catch (IOException io)
            {
                Console.WriteLine(io.ToString());
                Console.ReadLine();
            }

            //return tempWriter;
            return logWriterName;
        }

/*        public SQLiteDataReader GetLoginAccount(SQLiteConnection conn, string accountName)// string connString, string accountName)
        {
            //Console.WriteLine(connString);
//            using SQLiteConnection conn = new SQLiteConnection(connString);
//            conn.Open();

            string loginAccount = "SELECT * from accounts WHERE accountName = @accountName";// {accountName}";

            SQLiteCommand cmd = new SQLiteCommand(loginAccount, conn);
            cmd.Parameters.AddWithValue("@accountName", $"{accountName}");
            //Console.WriteLine(cmd);
            SQLiteDataReader data;
            try
            {
                data = cmd.ExecuteReader(CommandBehavior.SingleResult);
                //Console.WriteLine("trying to find a match");
                //return cmd.ExecuteReader(CommandBehavior.SingleResult);
                //if (data.Read())
                //{
                //    Console.WriteLine(data["accountName"].ToString());
                //Console.WriteLine("found record?");
                //Console.WriteLine(data);
                return data;
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
                //conn.Close();
                //Console.WriteLine("leaving lookup");
            }
            //Console.WriteLine(cmd.ExecuteReader(CommandBehavior.SingleResult));
            //Console.WriteLine(data);
            //return cmd.ExecuteReader(CommandBehavior.SingleResult);
            //conn.Close();
            return null;
        }*/

        [Serializable]//?
        public class AppSettings
        {
            public string serverName = "testServer";
            public string gameName = "default ONNAMud game";
            public string auth = "blahblahpassword";
            public string primaryAdminAccount = "admin";
            //public string ipOrDomain = "localhost";
            public int port = 11000;
            public bool debug = false;//whether or not to start with debug messages/etc on or off. primary for test server?
            public Guid FirstRoom { get; set; }//this is the room that new characters start in, character creator room
            //public string defaultServers = "";
            public string sqliteConn = "";
            //public List<SettingsServers> servers = new List<SettingsServers>();

        }

        [Serializable]
        public class SettingsServers
        {
            public string serverName = "default";
            //public string ipOrDomain = "localhost";
            //public int port = 11000;
            //public int serverPort = 11020;
            public string type = "login";//or "game"
            public bool debug = false;//whether or not to start with debug messages/etc on or off. primary for test server?
            //public string access = "default"; //I WAS gonna use this to set certain servers as default for connections. now jsut gonna do this when making an account
            //public string sqliteConn = "";
        }

        public static string Hash(string input)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, _iterations, _algorithm, _keySize);
            return string.Join(segmentDelimiter, Convert.ToHexString(hash), Convert.ToHexString(salt), _iterations, _algorithm);
        }

        public static bool Verify(string input, string hashString)
        {
            string[] segments = hashString.Split(segmentDelimiter);
            byte[] hash = Convert.FromHexString(segments[0]);
            byte[] salt = Convert.FromHexString(segments[1]);
            int iterations = int.Parse(segments[2]);
            HashAlgorithmName algorithm = new HashAlgorithmName(segments[3]);
            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, algorithm, hash.Length);
            return CryptographicOperations.FixedTimeEquals(inputHash, hash);

        }

        public class AccountSettings
        {
            public AccountType accountType;
            public int numOfAllowedChars;//the number of characters they can have created at once
            public int numOfOnlineChars;//the number of characters from this account they can have connected at the same time

            public AccountSettings(AccountType type, int allowed, int online)
            { 
                accountType = type;
                numOfAllowedChars = allowed;
                numOfOnlineChars = online;
            }

        }
    }
}
