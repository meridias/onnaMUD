using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using onnaMUD.Accounts;
using onnaMUD.Characters;
using onnaMUD.Database;
using onnaMUD.MUDServer;
using onnaMUD.Settings;

namespace onnaMUD.Utilities
{
    public static class Extensions
    {
        public static void MakeNewTrial(this ITemp iTemp, Player player)
        {
          /*  for (int i = 0; i < newTrials.Count; i++)
            {
                if (newTrials[i].player == player)
                {
                    if (newTrials[i].account.Length > 0 && newTrials[i].password.Length > 0)
                    {
                        //if the name and password are both longer than 0, meaning we've put something in there, then we're good?
                        Account account = new Account();
                        account.Id = DB.DBAccess.GetNewGuid<Account>(DB.Collections.Account);
                        account.AccountName = newTrials[i].account;
                        account.HashedPassword = Config.Hash(newTrials[i].password);
                        account.AccountType = AccountType.Trial;
                        List<Server> servers = DB.DBAccess.GetList<Server>(DB.Collections.Server);
                        for (int j = 0; j < servers.Count; j++)
                        {
                            if (servers[j].IsDefault && servers[j].Type == "game")
                            {
                                account.AllowedServers += $",{servers[j].Name}";
                            }
                        }
                        //got all the default servers, strip the first ','
                        if (account.AllowedServers.IndexOf(',') == 0)
                        {
                            //if the first ',' is at index 0, remove just that
                            account.AllowedServers = account.AllowedServers.Remove(0, 1);
                        }
                        DB.DBAccess.Save(account, DB.Collections.Account);
                        player.AccountID = account.Id;
                    }
                }
            }*/
        }



    }
    public class Temps
    {
        public static List<Player> playersWaitingOn = new List<Player>();
        //public static List<NewTrial> newTrials = new List<NewTrial>();
        public static List<TempFlag> trackedTemps = new List<TempFlag>();

        /// <summary>
        /// Check for temp methods, returns true if we want to keep checking command for normal command ops, false to stop checking
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CheckLists(Player player, string[] message)
        {
            //check the specific lists to see if this player has a pending temp... thing and if they do, process
            //if not, send back true to continue processing command with the normal command scripts
            switch (message[0])
            {

             /*   case "trial":
                    for (int i = 0; i < trackedTemps.Count; i++)
                    {
                        if (trackedTemps[i].player == player)
                        {



                        }
                        return true;//didn't find player in trackedTemp
                    }


                    for (int i = 0; i < newTrials.Count; i++)
                    {
                        if (newTrials[i].player == player)
                        {
                            //if this player is found in this list, then they have a temp running
                            string option = "";
                            string nameOrPass = "";
                            for (int j = 0; j < message.Length; j++)
                            {//get the possible options from message array
                                if (j == 1)
                                {
                                    option = message[j];
                                }
                                if (j == 2)
                                {
                                    nameOrPass = message[j];
                                }
                            }

                            switch (option.ToLower())
                            {//since we're here, we're assuming that we've started the new trial process
                                case "accept":
                                    if (newTrials[i].isReady)
                                    {//if account name and password have been set
                                        Account newAccount = new Account();
                                        newAccount.AccountName = newTrials[i].account;
                                        newAccount.HashedPassword = Config.Hash(newTrials[i].password);
                                        newAccount.AccountType = AccountType.Trial;
                                        newAccount.AllowedServers = Config.config.defaultServers;
                                        newAccount.Id = DB.DBAccess.GetNewGuid<Account>(DB.Collections.Account);

                                        DB.DBAccess.CreateAccount(newAccount);// newTrial.account);
                                        RemoveTrialProcess(player);
                                        //ServerFunctions.NewTrialAccount(newTrial, "remove");

                                        ServerFunctions.SendData(player, "You have created a new trial account! Please enjoy the game!");
                                        player.Id = newAccount.Id;
                                        return false;
                                        //return;
                                    }
                                    else
                                    {//if account name and password aren't done yet
                                        ServerFunctions.SendData(player, "You haven't finished setting your account name and password yet.");
                                        goto case "check";
                                    }
                                //break;
                                case "redo":
                                    newTrials[i].account = "";
                                    newTrials[i].password = "";
                                    newTrials[i].password1 = "";
                                    newTrials[i].isReady = false;
                                    break;
                                case "account":
                                    if (!newTrials[i].isReady)
                                    {
                                        if (nameOrPass.Length < 8)
                                        {
                                            ServerFunctions.SendData(player, "Your account name needs to be at least 8 characters long. Please try again.");
                                            return false;
                                            //return;
                                        }

                                        bool isUsedAccountName = DB.DBAccess.CheckAccountName(nameOrPass);

                                        if (!isUsedAccountName)
                                        {
                                            newTrials[i].account = nameOrPass;
                                            ServerFunctions.SendData(player, $"{nameOrPass} is a valid new account name. If you don't like this one, please choose another with TRIAL ACCOUNT youraccountnamehere");
                                            if (newTrials[i].password1 == "")//we check the second one so we know if we've entered password twice or not
                                            {//account name is set but password isn't
                                                ServerFunctions.SendData(player, "The next step in making your account is to set your password. Please enter your password by typing TRIAL PASSWORD yourpasswordhere");
                                                return false;
                                            }
                                            else
                                            {//account name is set and so is password
                                                newTrials[i].isReady = true;
                                                goto case "check";
                                            }
                                            //return;
                                        }
                                        else
                                        {
                                            ServerFunctions.SendData(player, $"{nameOrPass} is not a valid new account name. Please choose another one.");
                                            newTrials[i].account = "";
                                            return false;
                                            //return;
                                        }
                                    }
                                    goto case "check";
                                //break;
                                case "password":
                                    if (!newTrials[i].isReady)
                                    {
                                        //new trial isn't done yet
                                        if (nameOrPass.Length > 0)
                                        {
                                            //they have something for a password here
                                            if (newTrials[i].password.Length == 0)
                                            {
                                                //entered password, first time
                                                newTrials[i].password = nameOrPass;
                                                ServerFunctions.SendData(player, "Please re-enter your password with TRIAL PASSWORD yourpasswordhere");
                                                return false;
                                            }
                                            else
                                            {
                                                //entering password, second input
                                                //if (newTrials[i].password1 == "")
                                                //{
                                                    newTrials[i].password1 = nameOrPass;
                                                //}

                                                if (newTrials[i].password != newTrials[i].password1)
                                                {
                                                    //passwords don't match
                                                    ServerFunctions.SendData(player, "Your entered passwords don't match. Please enter your password with TRIAL PASSWORD yourpasswordhere");
                                                    newTrials[i].password = newTrials[i].password1 = "";
                                                    return false;
                                                    //return;
                                                }
                                                else
                                                {//passwords match, checking for account name and we're done?
                                                    if (newTrials[i].account != "")//we've just set the password and the account name is set
                                                    {
                                                        newTrials[i].isReady = true;
                                                        goto case "check";
                                                    }
                                                    else
                                                    {//we've just set the password, but the account name is not set
                                                        ServerFunctions.SendData(player, "You haven't entered a valid account name yet. Please type TRIAL ACCOUNT youraccountnamehere");
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        //else
                                        //{//nothing entered for a password and new trial isn't done yet
                                        //goto default;
                                        // }
                                    }
                                    //else
                                    //{
                                    //new trial is ready but they put in trial password again anyway?
                                    goto case "check";
                                //}

                                //break;
                                case "check":
                                    if (newTrials[i].isReady)
                                    {
                                        //if account name and password are both done and whatever we entered was not accept or redo
                                        ServerFunctions.SendData(player, $"You have entered {newTrials[i].account} for your account name and {newTrials[i].password} as your password.");
                                        ServerFunctions.SendData(player, "If you accept these as they are, please type TRIAL ACCEPT. If not, type TRIAL REDO");
                                        //return false;
                                    }
                                    else
                                    {
                                        //either account name or password aren't set yet
                                        if (newTrials[i].account == "")
                                        {
                                            ServerFunctions.SendData(player, "Please type TRIAL ACCOUNT youraccountnamehere to set your account name.");
                                            //return false;
                                        }
                                        if (newTrials[i].password1 == "")
                                        {//we haven't entered password twice yet, have we even entered it once yet?
                                            if (newTrials[i].password == "")
                                            {
                                                //haven't entered password once yet
                                                ServerFunctions.SendData(player, "Please type TRIAL PASSWORD yourpasswordhere to set your password.");
                                                //return false;
                                            }
                                            else
                                            {
                                                //we've entered it once, but not twice to check it
                                                ServerFunctions.SendData(player, "Please re-enter your password with TRIAL PASSWORD yourpasswordhere");
                                                //return;
                                            }
                                        }
                                    }
                                    return false;
                                    //break;
                                default:
                                    return true;

                                    //no 'option' set, or check accountname/passwords?
                                    //return true;
                                    //break;
                            }

                            break;
                        }
                    }
                    break;*/
                default:
                    //if message[0](the command) doesn't match the switch, then either typo or there's no temp for that command?
                    break;

            }


            return true;
        }

        /// <summary>
        /// Is this player in the trackedTemp list with this temp tag?
        /// </summary>
        /// <param name="player"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        public static bool IsTrackedTemp(Player player, string temp)
        {
            for (int i = 0; i < trackedTemps.Count; i++)
            {
                if (trackedTemps[i].player == player && trackedTemps[i].trackedTemp == temp)
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddTrackedTemp(Player player, string temp)
        {
            for (int i = 0; i < trackedTemps.Count; i++)
            {
                //make sure this isn't in the list already
                if (trackedTemps[i].player == player && trackedTemps[i].trackedTemp == temp)
                {
                    return;
                }
            }

            TempFlag tempFlag = new TempFlag();
            tempFlag.player = player;
            tempFlag.trackedTemp = temp;
            trackedTemps.Add(tempFlag);
        }

 /*       /// <summary>
        /// Did this player get added to the new trial process list?
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool NewTrialProcess(Player player)
        {
            //check to see if this player is in the list already
            for (int i = 0; i < newTrials.Count; i++)
            {
                if (newTrials[i].player == player)
                {
                    //this player is in the list already
                    ServerFunctions.SendData(player, "You have already started making a new trial account.");
                    return true;
                }
            }
            //not in the list
            NewTrial trial = new NewTrial();
            trial.player = player;
            newTrials.Add(trial);
            ServerFunctions.SendData(player, "054", "open");
            ServerFunctions.SendData(player, "Thank you for wanting to give this game a try! In order to get your trial account set up, please set your account name and password in the provided window.");
            return false;
        }*/

  /*      public static void MakeNewTrial(Player player)
        {
            for (int i = 0; i < newTrials.Count; i++)
            {
                if (newTrials[i].player == player)
                {
                    if (newTrials[i].account.Length > 0 && newTrials[i].password.Length > 0)
                    {
                        //if the name and password are both longer than 0, meaning we've put something in there, then we're good?
                        Account account = new Account();
                        account.Id = DB.DBAccess.GetNewGuid<Account>(DB.Collections.Account);
                        account.AccountName = newTrials[i].account;
                        account.HashedPassword = Config.Hash(newTrials[i].password);
                        account.AccountType = AccountType.Trial;
                        List<Server> servers = DB.DBAccess.GetList<Server>(DB.Collections.Server);
                        for (int j = 0; j < servers.Count; j++)
                        {
                            if (servers[j].IsDefault && servers[j].Type == "game")
                            {
                                account.AllowedServers += $",{servers[j].Name}";
                            }
                        }
                        //got all the default servers, strip the first ','
                        if (account.AllowedServers.IndexOf(',') == 0)
                        {
                            //if the first ',' is at index 0, remove just that
                            account.AllowedServers = account.AllowedServers.Remove(0, 1);
                        }
                        DB.DBAccess.Save(account, DB.Collections.Account);
                        player.AccountID = account.Id;
                    }
                }
            }
        }*/

 /*       public static void SetNewTrialInfo(Player player, string name, string password)
        {
            for (int i = 0; i < newTrials.Count; i++)
            {
                if (newTrials[i].player == player)
                {
                    newTrials[i].account = name;
                    newTrials[i].password = password;
                }
            }

        }*/

 /*       public static void RemoveTrialProcess(Player player)
        {
            for (int i = 0; i < newTrials.Count; i++)
            {
                if (newTrials[i].player == player)
                {
                    newTrials.RemoveAt(i);
                }
            }
        }*/

        public class NewTrial
        {
            //public TcpClient? client = null;//since we don't have an account name yet, have to use the client to check against
            public Player? player;
            //public Account account = new Account();
            //public bool isReady = false;
            public string account = "";
            public string password = "";
            //public string password1 = "";
            //public Guid clientGUID;//so we can check against the client guid and this new trial 'quest' to create a new trial account so we can keep track
            //public string account = "";
            //public string password = "";
            //public bool isAccountName = false;//when account name has been verified by user
            //public bool isPassword = false;//when password has been verified by user
        }

        public class TempFlag
        {
            public Player player;
            public string trackedTemp;
            public bool isReady = false;
        }

    }
}
