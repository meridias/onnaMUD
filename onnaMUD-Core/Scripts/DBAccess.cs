using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using onnaMUD.Settings;
//using onnaMUD.Accounts;
using onnaMUD.Characters;
using onnaMUD.MUDServer;
using System.Data;
using Microsoft.EntityFrameworkCore;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
using LiteDB;
using System.Collections;
using Newtonsoft.Json;

namespace onnaMUD.Database
{

    public sealed class DB
    {
        private static DB instance = null;
        private static LiteDatabase db;
        private DB() { }

        public static DB DBAccess
        {
            get
            {
                if (instance == null)
                {
                    instance = new DB();
                }
                return instance;
            }
        }
        
        public static void CheckForLocalDB()
        {
            //var mapper = BsonMapper.Global
            //this SHOULD create the database if it isn't already
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "onnaMUD.db");
            db = new LiteDatabase(dbPath);//, mapper);

            if (!DBAccess.DoesCollectionExist(Collections.Account))
            {//if the account collection doesn't exist, then add primary admin
                Account tempAccount = new Account();
                tempAccount.AccountName = Config.config.primaryAdminAccount;
                tempAccount.HashedPassword = Config.Hash(Config.config.auth);
                tempAccount.AccountType = AccountType.Owner;
                //var coll = DBAccess.GetCollection<Account>(Collections.Account);
                //coll.EnsureIndex(x => x.AccountID);
                DBAccess.Save(tempAccount, Collections.Account);

                //create primary admin character on admin account
                Character adminChar = new Character();
                adminChar.Name = "MrNobody";
                adminChar.AccountID = DBAccess.GetLoginAccount(Config.config.primaryAdminAccount).Id;
                DBAccess.Save(adminChar, Collections.Character);
            }

            //using (ServerContext db = new ServerContext())
            //{
              //  string serverDatabasePath = Directory.GetCurrentDirectory();
              //  string serverDatabaseFile = Path.Combine(serverDatabasePath, "onnaMUD.db");
                //      if (!File.Exists(serverDatabaseFile))
                //      {
                //ServerConsole.newConsole.WriteLine(serverDatabaseFile);
                //  if (!File.Exists(serverDatabaseFile))
                //  {
                //      SQLiteConnection.CreateFile(serverDatabaseFile);
                //  }
          /*      db.Database.EnsureCreated();

                Account adminAccount = GetLoginAccount(Config.config.primaryAdminAccount);
                if (adminAccount.AccountName == "moo")//default name, means admin name doesn't exist
                {
                    db.Accounts.Add(new Account
                    {
                        AccountName = Config.config.primaryAdminAccount,
                        HashedPassword = Config.Hash(Config.config.auth),
                        AccountType = AccountType.Admin

                    });
                    db.SaveChanges();
                }*/
                //      }
          //  }
        }

        public Account GetLoginAccount(string accountName)
        {
            try
            {
                var account = GetCollection<Account>(Collections.Account).FindOne(a => a.AccountName == accountName);//   FindById(accountName);
                if (account == null)
                {
                    return new Account();
                }
                else
                {
                    //var account = GetCollection<Account>(Collections.Account).First(a => a.AccountName == accountName);
                    return account;
                }
            }
            catch (Exception)// ex)
            {
                //Console.WriteLine(ex.InnerException.Message);
                return new Account();
            }
        }

        public string GetInfo(Player player, string infoToGet)
        {
            switch (infoToGet)
            {
                case "allowedServers":
                    try
                    {
                        var account = DBAccess.GetCollection<Account>(Collections.Account).FindById(player.AccountID);// db.Accounts.First(a => a.AccountID == player.AccountID);
                        if (account != null && !string.IsNullOrEmpty(account.AllowedServers))
                        {
                            return account.AllowedServers;
                        } else
                        {
                            return "";
                        }
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                //break;

                default:
                    return "";
            }
        }

        public List<Character> GetCharacters(Player player, int serverIndex)
        {
            List<Character> list = new List<Character>();
 //           string name = ServerFunctions.servers[serverIndex].serverInfo.Name;
 //           list = GetCharacters(player, name);
            return list;
        }

        public List<Character> GetCharacters(Player player, string serverName)// int serverIndex)// Guid accountID)//  int accountID)
        {
            List<Character> list = new List<Character>();
            try
            {
                if (player.AccountType >= AccountType.Mod)
                {
                    //just return all characters since we're mod/admin and it doesn't matter what server
                    list = DBAccess.GetCollection<Character>(Collections.Character).FindAll().Where(c => c.AccountID.Equals(player.AccountID)).ToList();
//                    return list;
                } else
                {
                    list = DBAccess.GetCollection<Character>(Collections.Character).FindAll()
                        .Where(c => c.AccountID.Equals(player.AccountID) && c.Server.Equals(serverName)).ToList();// ServerFunctions.servers[serverIndex].serverName)).ToList();
//                    return list;
                }
                //sort the character list by name
                list = list.OrderBy(x => x.Name).ToList();
                return list;

                //              list = DBAccess.GetCollection<Character>(Collections.Character).FindAll()
                //                .Where(c => c.AccountID.Equals(player.AccountID)).ToList();// db.Characters.Where(c => c.AccountID.Equals(accountID)).ToList();// FirstOrDefault(a => a.AccountName == accountName);
                //          return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                return list;
            }
            //               var blogs = context.Blogs
            //   .Where(b => b.Url.Contains("dotnet"))
            //   .ToList();

        }

        public Guid GetNewGuid<T>(Collections collName)//  string whichTable)
        {
            Guid newGuid = Guid.NewGuid();
            bool usedGuid = true;
            //this is the group of collections that need to have unique guids among them
            Collections[] collGroup = new[] { Collections.RoomObject, Collections.Character, Collections.NPC };

            //gonna need to check these for unique guid across these collections:
            //characters, objects, items, npc
            //because these all could be in the room in the 'You also see' list and I don't want to have to specify which guid collection to check for each one
            //just get a guid, go through the collections til a match, and we're good
            //need to change that. since all things are type Thing, which we can make a list of, we don't need to worry about having any conflicting guids


            //everything else can be unique to themselves

  //          string collection = CollectionName(collName);

  //          switch (collection)
    //        {
  /*              case "Character":
                case "NPC":
                case "Object":
                    while (usedGuid)
                    {
                        for (int i = 0; i < collGroup.Length; i++)
                        {
                            //if the collection exists, then check it
                            if (DoesCollectionExist(collGroup[i]))
                            {
                                var coll = DBAccess.GetCollection<T>(collGroup[i]).FindById(newGuid);
                                if (coll != null)
                                {
                                    newGuid = Guid.NewGuid();
                                    break;
                                }
                            }
                        }
                        //if we got to this point, SHOULD have not found a match
                        usedGuid = false;
                    }
                    break;*/
 //               default:
                    while (usedGuid)
                    {
                        //if collection exists, check it
                        if (DoesCollectionExist(collName))
                        {
                            var coll = DBAccess.GetCollection<T>(collName).FindById(newGuid);
                            if (coll != null)
                            {
                                newGuid = Guid.NewGuid();
                            }
                            else
                            {
                                usedGuid = false;
                            }
                        }
                        else
                        {
                            usedGuid = false;
                        }
                    }
  //                  break;
   //         }

            //var collection = _db.GetCollection<T>(GetCollectionName(collectionName));
            //var collection = DBAccess.GetCollection<T>(collName);
            //var collection = DBAccess.GetCollection<T>(collName).FindById(newGuid);

  /*          while (usedGuid)
            {
                var collection = DBAccess.GetCollection<T>(collName).FindById(newGuid);
                if (collection != null)
                {
                    newGuid = Guid.NewGuid();
                } else
                {
                    usedGuid = false;
                }

            }*/

      /*      switch (whichTable)
            {
                case "accounts":
                    while (usedGuid)
                    {
                        var account = GetCollection<Account>(Collections.Account).FindById(newGuid);// db.Accounts.Any(g => g.AccountID == newGuid);
                        if (account != null)//usedGuid)
                        {//if not null, then found match to newGuid, so pick a new one
                            newGuid = Guid.NewGuid();
                        } else
                        {//match is null, so newGuid is new
                            usedGuid = false;
                        }
                    }
                    break;
            }*/
            return newGuid;
        }

        public bool CheckAccountName(string newAccountName)
        {
            try
            {
                var account = DBAccess.GetCollection<Account>(Collections.Account).FindOne(a => a.AccountName == newAccountName);//  db.Accounts.Any(a => a.AccountName == newAccountName);
                if (account != null)
                {//found match so account name is being used
                    return true;
                } else
                {
                    return false;
                }
                //return true;// result;

                //bool result = entities.Any(x => x.Id == id && x.Title == title);
                //Account account = db.Accounts.First(a => a.AccountName == newAccountName);
                //if (account.AccountName.Length > 0)
                // {
                //   return true;
                // }
                // else
                // {
                //     return false;
                // }
                /*       if (account.AccountName == "moo")
                       {//moo is default account name so newAccountName does not yet exist
                           return false;
                       }
                       else
                       {
                           return true;
                       }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                return true;//meaning, we're not sure if newAccountName exists or not, so we're assuming yes just to not mess anything up
            }
        }

        public void CreateAccount(Account newAccount)
        {
            try
            {
                DBAccess.Save(newAccount, Collections.Account);
                //db.Accounts.Add(newAccount);
                //db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                return;
            }
        }

        public bool IsUsedCharName(string charName)
        {
            var character = DBAccess.GetCollection<Character>(Collections.Character).FindOne(a => a.Name.ToLower() == charName.ToLower());
            if (character != null)
            {//found match so character name is being used
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Save<T>(T data, Collections collName)
        {
            var collection = db.GetCollection<T>(CollectionName(collName));
            //collection.Upsert(data);
            try
            {
                //ServerConsole.newConsole.WriteLine("collection type: "+collection.GetType().Name);
                if (data.GetType().Name == "Player")
                {
                    //yes, this is ugly. but converting the player class to a json string then deserializing it back to a character class is the easiest?
                    //way to get the base character class from player class
                    data = (T)(object)JsonConvert.DeserializeObject<Character>(JsonConvert.SerializeObject(data));
                }
                //ServerConsole.newConsole.WriteLine("data type: "+data.GetType().Name);
                collection.Upsert(data);
                db.Checkpoint();
            }
            catch (Exception ex)
            {
                ServerConsole.newConsole.WriteLine(ex);
            }
            return true;
        }

        public LiteCollection<T> GetCollection<T>(Collections collName)
        {
            return (LiteCollection<T>)db.GetCollection<T>(CollectionName(collName));
        }

        public List<T> GetList<T>(Collections collName)
        {
            return db.GetCollection<T>(CollectionName(collName)).FindAll().ToList();
        }

        public T GetById<T>(Guid id, Collections collectionName)
        {
            var record = db.GetCollection<T>(CollectionName(collectionName)).FindById(id);
            return record;
        }

        public bool DoesCollectionExist(Collections collName)
        {
            var doesExist = db.CollectionExists(CollectionName(collName));
            return doesExist;
        }

        public enum Collections
        {
            Account,
            Character,//player characters
            NPC,//unique npc characters, with their npc/ai specifics
            RoomObject,
            Region,
            Area,
            Room,
            Server
        }

        public static string CollectionName(Collections collName)
        {
            return collName switch
            {
                Collections.Account => "Account",
                Collections.Character => "Character",
                Collections.NPC => "NPC",
                Collections.RoomObject => "RoomObject",
                Collections.Region => "Region",
                Collections.Area => "Area",
                Collections.Room => "Room",
                Collections.Server => "Server",
            };
        }

    }

    public class ServerContext : DbContext
    {
        //public DbSet<Account> Accounts { get; set; }
        //no, 
        //public DbSet<Player> PlayerChar { get; set; }
        //public DbSet<NPC> NPlayerChar { get; set; }

        //the character info for this character, regardless if pc or npc
        public DbSet<Character> Characters { get; set; }
        //public DbSet<Room> Rooms { get; set; }
        //public DbSet<RoomObject> Objects { get; set; }


        public string DbPath { get; }
        public ServerContext()
        {
            string serverDatabasePath = Directory.GetCurrentDirectory();
            DbPath = Path.Combine(serverDatabasePath, "onnaMUD.db");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

    }



    public class DBAccess1
    {
        //public static DBAccess instance = new DBAccess();

        //private string serverDatabasePath = Directory.GetCurrentDirectory();
        //private static string serverDatabaseFile;
        //private string connString;
        //private static SQLiteConnection conn;

        public DBAccess1()
        {
            //instance = this;
            //string connString = $"Data Source={serverDatabaseFile}";
            //conn = new SQLiteConnection(connString);
            //Console.WriteLine(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "onnaMUD.db")));
        }

        public static void CheckForLocalDB()
        {
            using (ServerContext db = new ServerContext())
            {
                string serverDatabasePath = Directory.GetCurrentDirectory();
                string serverDatabaseFile = Path.Combine(serverDatabasePath, "onnaMUD.db");
                //      if (!File.Exists(serverDatabaseFile))
                //      {
                //ServerConsole.newConsole.WriteLine(serverDatabaseFile);
              //  if (!File.Exists(serverDatabaseFile))
              //  {
              //      SQLiteConnection.CreateFile(serverDatabaseFile);
              //  }
                db.Database.EnsureCreated();

                Account adminAccount = GetLoginAccount(Config.config.primaryAdminAccount);
                if (adminAccount.AccountName == "moo")//default name, means admin name doesn't exist
                {
                 /*   db.Accounts.Add(new Account
                    {
                        AccountName = Config.config.primaryAdminAccount,
                        HashedPassword = Config.Hash(Config.config.auth),
                        AccountType = AccountType.Admin

                    });*/
                    db.SaveChanges();
                }
                //      }
            }
        }

        public static Account GetLoginAccount(string accountName)
        {
            using (ServerContext db = new ServerContext())
            {
                try
                {
                    var account = new Account();// = db.Accounts.First(a => a.AccountName == accountName);
                    return account;
                }
                catch (Exception)// ex)
                {
                    //Console.WriteLine(ex.InnerException.Message);
                    return new Account();
                }
 //               var blog = db.Blogs
 //   .OrderBy(b => b.BlogId)
 //   .First();
            }
        }

        public static string GetInfo(Player player, string infoToGet)
        {
            using (ServerContext db = new ServerContext())
            {
                switch (infoToGet)
                {
                    case "allowedServers":
                        try
                        {
                            //var account = db.Accounts.First(a => a.AccountID == player.AccountID);
                            return "blah";// account.AllowedServers;
                        }
                        catch (Exception)
                        {
                            return "";
                        }
                    //break;

                    default:
                        return "";
                }
            }
        }

        public static bool CheckAccountName(string newAccountName)
        {
            using (ServerContext db = new ServerContext())
            {
                try
                {
                    //bool result = db.Accounts.Any(a => a.AccountName == newAccountName);
                    return true;// result;

                    //bool result = entities.Any(x => x.Id == id && x.Title == title);
                    //Account account = db.Accounts.First(a => a.AccountName == newAccountName);
                    //if (account.AccountName.Length > 0)
                   // {
                     //   return true;
                   // }
                   // else
                   // {
                   //     return false;
                   // }
             /*       if (account.AccountName == "moo")
                    {//moo is default account name so newAccountName does not yet exist
                        return false;
                    }
                    else
                    {
                        return true;
                    }*/
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    return true;//meaning, we're not sure if newAccountName exists or not, so we're assuming yes just to not mess anything up
                }
            }
        }

        public List<Character> GetCharacters(Guid accountID)//  int accountID)
        {
            using (ServerContext db = new ServerContext())
            {
                List<Character> list = new List<Character>();
                try
                {
                    list = db.Characters.Where(c => c.AccountID.Equals(accountID)).ToList();// FirstOrDefault(a => a.AccountName == accountName);
                    return list;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    return list;
                }
                //               var blogs = context.Blogs
                //   .Where(b => b.Url.Contains("dotnet"))
                //   .ToList();
            }
        }

        public void CreateAccount(Account newAccount)
        {
            using (ServerContext db = new ServerContext())
            {
                try
                {
//                    db.Accounts.Add(newAccount);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    return;
                }
            }
        }

        public bool IsNewAccountGuid(Guid newGuid)
        {
            using (ServerContext db = new ServerContext())
            {
                try
                {
                    bool isNotNew = true;// = db.Accounts.Any(a => a.AccountID == newGuid);//if this is true, then we found a match to the guid
                    return !isNotNew;//return the opposite of if we found a match
                    //if the query returned false for no match, then return true that it's a new guid
                    //and if true for a match, return false for not a new guid
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    return true;//not sure so we'll send true just to not mess anything up
                }
            }
        }

        public Guid GetNewGuid(string whichTable)
        {
            using (ServerContext db = new ServerContext())
            {
                Guid newGuid = Guid.NewGuid();
                bool usedGuid = true;

                switch (whichTable)
                {
                    case "accounts":
                        while (usedGuid)
                        {
                            usedGuid = true;// db.Accounts.Any(g => g.AccountID == newGuid);
                            if (usedGuid)
                            {
                                newGuid = Guid.NewGuid();
                            }
                        }
                        break;
                }
                return newGuid;
            }
        }


         /*       if (!File.Exists(serverDatabaseFile))
                {
                    conn.Open();

                    string accountTable = "CREATE TABLE accounts (accountID INTEGER PRIMARY KEY AUTOINCREMENT, accountName TEXT, hashedPassword TEXT, accountType TEXT, allowedServers TEXT)";
                    //Console.WriteLine("Creating accounts table...");
                    SQLiteCommand cmd = new SQLiteCommand(accountTable, conn);//creating accounts table
                    cmd.ExecuteNonQuery();

                    //inserting primary admin account
                    //doing it this way with AddWithValue is apparently supposed to help against sql injection, per: IronPDF.com "c# sqlite (how it works for developer)"
                    //Console.WriteLine("Inserting primary admin account...");
                    string adminAccount = "INSERT INTO accounts (accountName, hashedPassword, accountType) VALUES (@accountName, @password, @type)";
                    cmd = new SQLiteCommand(adminAccount, conn);
                    cmd.Parameters.AddWithValue("@accountName", $"{Config.config.primaryAdminAccount}");//  config["primaryAdminAccount"]}");
                    cmd.Parameters.AddWithValue("@password", Config.Hash(Config.config.auth));//  config["auth"]));
                    cmd.Parameters.AddWithValue("@type", "admin");
                    //Console.WriteLine(cmd.ExecuteNonQuery());
                    cmd.ExecuteNonQuery();

                    string charTable = "CREATE TABLE characters (charID INTEGER PRIMARY KEY AUTOINCREMENT, charName TEXT, accountID INTEGER)";
                    //Console.WriteLine("Creating characters table...");
                    cmd = new SQLiteCommand(charTable, conn);//creating characters table
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }*/
        //}

        //return type WAS SQLiteDataReader
 /*       public static DataTable GetLoginAccount(string accountName)// SQLiteConnection conn, string accountName)// string connString, string accountName)
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
                conn.Open();
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
            catch (Exception)
            {
                Console.WriteLine("GetLoginAccount blah");
            }
            finally
            {
                conn.Close();
                //conn.Close();
                //Console.WriteLine("leaving lookup");
            }
            //Console.WriteLine(cmd.ExecuteReader(CommandBehavior.SingleResult));
            //Console.WriteLine(data);
            //return cmd.ExecuteReader(CommandBehavior.SingleResult);
            //conn.Close();
            return null;
        }*/

        public static DataTable GetCharListForAccount(int accID)
        {
            string getChar = "SELECT * from characters WHERE accountID = @accountID";// {accountName}";

//            SQLiteCommand cmd = new SQLiteCommand(getChar, conn);
//            cmd.Parameters.AddWithValue("@accountID", $"{accID}");

            try
            {
//                conn.Open();
//                SQLiteDataReader data = cmd.ExecuteReader();

                DataTable charData = new DataTable();
//                charData.Load(data);
                return charData;

            }
            catch (SQLiteException)
            {
                Console.WriteLine("error when getting a reader result");
            }
            catch (Exception)
            {
                Console.WriteLine("GetCharListForAccount blah");
            }
            finally
            {
//                conn.Close();
                //conn.Close();
                //Console.WriteLine("leaving lookup");
            }

            return null;

        }

    }

}
