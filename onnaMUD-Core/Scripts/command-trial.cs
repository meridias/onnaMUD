using System;
using onnaMUD.MUDServer;
using onnaMUD.Characters;
using onnaMUD.Temps;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
//using onnaMUD.Accounts;
using onnaMUD.Database;
using onnaMUD.Settings;
using onnaMUD.Utilities;
//using System.Security.Principal;

namespace onnaMUD.Utilities
{
    public class Trial : ICommand
    {
        //public Server server { get; set; }

        public Trial()
        {
            Aliases = new[] { "trial" };
            AllowedUsers = AccountType.Trial;
            AllowedInRT = true;
        }

        public string[] Aliases { get; }
        public AccountType AllowedUsers { get; }
        public bool AllowedInRT { get; }

        public void Execute(Player player, Room room, string[] command)//  Guid charGuid, string command)
        {
            if (player.AccountType > AccountType.Trial)
            {
                //isNewTrialStarted = server.IsTrialProcessStarted(player);
                ServerFunctions.SendData(player, "You already have a registered account.");
                return;
            }

            if (player.AccountID != Guid.Empty)
            {
                //if this player already has an accountID set, then they've already got a trial account started
                //show active time left on this account
                ServerFunctions.SendData(player, "You have blah playable time left on this account.");
                return;
            }

            //string[] split = message.Split(' ');
            string option = "";
            //string nameOrPass = "";
            //   bool isNewTrialStarted = ServerFunctions.IsTrialProcessStarted(player);//is this player already doing the process or not?
            //   ServerMain.NewTrial newTrial = ServerFunctions.NewTrialAccount(player);//get the newTrial for this player if they're in the list or start a new one for this player
            //Account newAccount = new Account(); //

            if (command.Length > 1)
            {
                option = command[1].ToLower();
            }
            //ServerFunctions.SendData(player, isNewTrialStarted.ToString());
            //ServerFunctions.SendData(player, newTrial.player.Guid.ToString());
            //Server.NewTrial newTrial = new Server.NewTrial();//check to see if we've already started the newtrial bit or use a default Trial to start with
            /*       if (server.newTrialAccounts.Count > 0)
                   {
                       for (int i = 0; i < server.newTrialAccounts.Count; i++)
                       {
                           if (server.newTrialAccounts[i].player == player)
                           {
                               newTrial = server.newTrialAccounts[i];
                               //isNewTrialStarted = true;//we already got this from the method just before this
                           }
                       }
                   }*/

            //      if (message.Length == 1 || message.Length > 3)
            //    {//    /trial by itself
            //      ServerFunctions.SendData(player, "To get information about a trial account, please type /trial info<br>To start a trial account, please type /trial start");
            //}

            switch (option)
            {
                case "info":
                    ServerFunctions.SendData(player, "A trial account lets you try out this game without having to register on the website. You are only able to create 1 character, although you can delete it and start another if you wish. The account will be active for 30 days, after which you can no longer play unless you register. If you haven't registered after 60 days from the start of the trial, the account and character will be deleted.");
                    ServerFunctions.SendData(player, "Some classes will be unavailable and some regions will be inaccessible.");
                    break;

                case "redo":
                    //Temps.SetNewTrialInfo(player, "", "");
                    player.tempScript.Clear();
                    ServerFunctions.SendData(player, "054", "open");
                    ServerFunctions.SendData(player, "Please set your account name and password in the provided window.");
                    break;
                case "accept":
                    ServerFunctions.SendData(player, "You have created a new trial account! Please enjoy the game!");
                    player.tempScript.Create(player);
                    //Temps.MakeNewTrial(player);
                    break;
                case "start":
                    //Temps.NewTrialProcess(player);
                    if (player.tempScript == null)
                    {
                        //starting the new trial process
                        ServerFunctions.SendData(player, "Thank you for wanting to give this game a try! In order to get your trial account set up, please set your account name and password in the provided window.");
                        //player.tempScript = new NewAccount(player);
                    }
                    else
                    {
                        //double check for 'NewAccount', just in case?
                        if (player.tempScript.GetType().Name == "NewAccount")
                        {
                            ServerFunctions.SendData(player, "You have already started making a new trial account.");
                        }
                        else
                        {
                            //somehow, something else besides NewAccount got set on tempScript. how did this happen on the login server?
                            ServerConsole.newConsole.WriteLine($"{player.IP} somehow got something else besides NewAccount in tempScript on login.");
                        }

                    }
                    //ServerConsole.newConsole.WriteLine(player.tempScript.GetType().Name);
                    //NewAccount blah = (NewAccount)player.tempScript;
                    //blah.Check("blah");
                    //player.tempScript.Account
                    //if this player isn't already added to the temp list
                    /*          if (!Temps.IsTrackedTemp(player, "trial"))
                              {
                                  Temps.AddTrackedTemp(player, "trial");
                                  ServerFunctions.SendData(player, "054", "open");
                                  ServerFunctions.SendData(player, "Thank you for wanting to give this game a try! In order to get your trial account set up, please set your account name and password in the provided window.");
                              } else
                              {
                                  ServerFunctions.SendData(player, "You have already started making a new trial account.");
                              }*/

                    //ServerFunctions.SendData(player, "054", "open");
                    //if (!isNewTrialStarted)
                    //{
                    //newTrial.player = player;
                    //newTrial.account = new Account();
                    /*       if (Temps.NewTrialProcess(player))
                           {
                               //this player just got added to the list
                               ServerFunctions.SendData(player, "Thank you for wanting to give this game a try! In order to get your trial account set up, please set your account name by typing TRIAL ACCOUNT youraccountnamehere and set your password by typing TRIAL PASSWORD yourpasswordhere");
                           }
                           else
                           {
                               //didn't get added, so assuming they're already in it?
                               ServerFunctions.SendData(player, "You have already started making a new trial account.");
                           }*/
                    //ServerFunctions.NewTrialAccount(newTrial, "add");
                    //ServerFunctions.SendData(player, "Thank you for wanting to give this game a try! In order to get your trial account set up, please set your account name by typing TRIAL ACCOUNT youraccountnamehere and set your password by typing TRIAL PASSWORD yourpasswordhere");
                    //return;
                    // } else
                    // {
                    //     ServerFunctions.SendData(player, "You have already started making a new trial account.");
                    //goto default;
                    // }
                    break;
                /*           case "accept":
                               if (newTrial.isReady)
                               {//if account name and password have been set
                                   Account newAccount = new Account();
                                   newAccount.AccountName = newTrial.account;
                                   newAccount.HashedPassword = Config.Hash(newTrial.password);
                                   newAccount.AccountType = AccountType.Trial;
                                   newAccount.AllowedServers = Config.config.defaultServers;
                                   newAccount.Id = DB.DBAccess.GetNewGuid<Account>(DB.Collections.Account);

                                   //newTrial.account.HashedPassword = Config.Hash(newTrial.account.HashedPassword);
                                   //newTrial.account.AccountType = AccountType.Trial;
                                   //newTrial.account.AllowedServers = Config.config.defaultServers;
                                   //newTrial.account.AccountID = DBAccess.GetNewGuid("accounts");
                                   //player.AccountID = newAccount.AccountID;

                                   DB.DBAccess.CreateAccount(newAccount);// newTrial.account);
                                   //ServerFunctions.NewTrialAccount(newTrial, "remove");

                                   ServerFunctions.SendData(player, "You have created a new trial account! Please enjoy the game!");
                                   player.Id = newAccount.Id;
                                   //return;
                               }
                               else
                               {//if account name and password aren't done yet
                                   ServerFunctions.SendData(player, "You haven't finished setting your account name and password yet.");
                                   goto default;
                               }
                               break;*/
                /*           case "redo":
                               if (newTrial.isReady)
                               {
                                   newTrial.account = "";
                                   newTrial.password = "";
                                   newTrial.isReady = false;
                                   goto default;
                               } else
                               {
                                   newTrial.account = "";
                                   newTrial.password = "";
                                   //ServerFunctions.SendData(player, "")
                                   goto default;
                               }*/
                //break;
                /*           case "account":
                               if (isNewTrialStarted)
                               {//we have started a new trial process
                                   if (!newTrial.isReady)
                                   {//the new trial process is not done yet
                                       if (message.Length > 2)
                                       {//we have a possible account name
                                        //           if (newTrial.account == "")//default account name
                                        //           {
                                           if (message[2].Length < 8)
                                           {
                                               ServerFunctions.SendData(player, "Your account name needs to be at least 8 characters long. Please try again.");
                                               //return;
                                           }

                                           bool isUsedAccountName = DB.DBAccess.CheckAccountName(message[2]);

                                           //if name check came back true, that means that it's already being used
                                           //so we need to find one that comes back false, so that the if will be true
                                           if (!isUsedAccountName)
                                           {
                                               newTrial.account = message[2];
                                               ServerFunctions.SendData(player, $"{message[2]} is a valid new account name. If you don't like this one, please choose another with TRIAL ACCOUNT youraccountnamehere");
                                               if (newTrial.password1 == "")//we check the second one so we know if we've entered password twice or not
                                               {//account name is set but password isn't
                                                   ServerFunctions.SendData(player, "The next step in making your account is to set your password. Please enter your password by typing TRIAL PASSWORD yourpasswordhere");
                                               } else
                                               {//account name is set and so is password
                                                   newTrial.isReady = true;
                                                   goto default;
                                               }
                                               //return;
                                           }
                                           else
                                           {
                                               ServerFunctions.SendData(player, $"{message[2]} is not a valid new account name. Please choose another one.");
                                               newTrial.account = "";
                                               //return;
                                           }
                                           //               }
                                       }
                                       else
                                       {
                                           goto default;
                                           //no possible account name
               //                            if (newTrial.account == "")
               //                            {//no typed account name and one hasn't been set yet
               //                                ServerFunctions.SendData(player, "Please choose an account name by typing TRIAL ACCOUNT youraccountnamehere");
               //                            } else
               //                            {//no typed account name and one is already set
               //                                ServerFunctions.SendData(player, $"Your currently chosen account name is {newTrial.account}. If you don't like this one, please choose another with TRIAL ACCOUNT youraccountnamehere");
               //                            }
                                       }
                                   } else
                                   {//the player typed trial account but it's already ready to go?
                                       goto default;
                                   }
                               } else
                               {
                                   ServerFunctions.SendData(player, "Please type TRIAL START if you would like to start a new trial account or TRIAL INFO for more information.");
                               }
                               break;*/
                /*           case "password":
                               if (isNewTrialStarted)
                               {
                                   if (!newTrial.isReady)
                                   {
                                       //new trial isn't done yet
                                       if (message.Length > 2)
                                       {
                                           //they have something for a password here
                                           if (newTrial.password.Length == 0)
                                           {
                                               //entered password, first time
                                               newTrial.password = message[2];
                                               ServerFunctions.SendData(player, "Please re-enter your password with TRIAL PASSWORD yourpasswordhere");
                                           }
                                           else
                                           {
                                               //entering password, second input
                                               if (newTrial.password1 == "")
                                               {
                                                   newTrial.password1 = message[2];
                                               }

                                               if (newTrial.password != newTrial.password1)
                                               {
                                                   //passwords don't match
                                                   ServerFunctions.SendData(player, "Your entered passwords don't match. Please enter your password with TRIAL PASSWORD yourpasswordhere");
                                                   newTrial.password = newTrial.password1 = "";
                                                   //return;
                                               }
                                               else
                                               {//passwords match, checking for account name and we're done?
                                                   if (newTrial.account != "")//we've just set the password and the account name is set
                                                   {
                                                       newTrial.isReady = true;
                                                       goto default;
                                                   }
                                                   else
                                                   {//we've just set the password, but the account name is not set
                                                       ServerFunctions.SendData(player, "You haven't entered a valid account name yet. Please type TRIAL ACCOUNT youraccountnamehere");

                                                   }
                                                   //newTrial.isReady = true;
                                                   //ServerFunctions.SendData(player, $"You have entered {newTrial.account} for your account name and {newTrial.password} as your password.");
                                                   //ServerFunctions.SendData(player, "If you accept these as they are, please type TRIAL ACCEPT. If not, type TRIAL REDO");
                                                   //return;
                                               }
                                           }
                                       }
                                       else
                                       {//nothing entered for a password and new trial isn't done yet
                                           goto default;
                                       }
                                   } else
                                   {
                                       //new trial is ready but they put in trial password again anyway?
                                       goto default;
                                   }
                               } else
                               {
                                   ServerFunctions.SendData(player, "Please type TRIAL START if you would like to start a new trial account or TRIAL INFO for more information.");
                               }
                               break;*/
                default:
                    /*           if (isNewTrialStarted)
                               {//new trial process started
                                   if (!newTrial.isReady)
                                   {//new trial isn't ready yet
                                    // if (option != "")
                                    // {
                                    //ServerFunctions.SendData(player, newTrial.account.AccountName);
                                    //if there is something after /trial...

                                       // }
                                       // else
                                       // {
                                       //there is nothing after /trial
                                       if (newTrial.account == "")
                                       {
                                           ServerFunctions.SendData(player, "Please type TRIAL ACCOUNT youraccountnamehere to set your account name.");
                                           return;
                                       }
                                       if (newTrial.password1 == "")
                                       {//we haven't entered password twice yet, have we even entered it once yet?
                                           if (newTrial.password == "")
                                           {
                                               //haven't entered password once yet
                                               ServerFunctions.SendData(player, "Please type TRIAL PASSWORD yourpasswordhere to set your password.");
                                               return;
                                           } else
                                           {
                                               //we've entered it once, but not twice to check it
                                               ServerFunctions.SendData(player, "Please re-enter your password with TRIAL PASSWORD yourpasswordhere");
                                               return;
                                           }
                                       }
                                       //if we've gotten to this point, account name is set and password has been checked
                                       newTrial.isReady = true;
                                       goto default;

                                       //ServerFunctions.SendData(player, "Please type TRIAL START if you would like to start a new trial account or TRIAL INFO for more information.");
                                       //return;
                                       //  }
                                   }
                                   else
                                   {//if account name and password are both done and whatever we entered was not accept or redo
                                       ServerFunctions.SendData(player, $"You have entered {newTrial.account} for your account name and {newTrial.password} as your password.");
                                       ServerFunctions.SendData(player, "If you accept these as they are, please type TRIAL ACCEPT. If not, type TRIAL REDO");
                                       //return;
                                   }*/
                    //                } else
                    //                {//new trial process not started
                    ServerFunctions.SendData(player, "Please type TRIAL START if you would like to start a new trial account or TRIAL INFO for more information.");
                    //                }
                    break;

            }


            /*  if (!isNewTrialStarted)
              {//we haven't started the new trial account process yet
                  switch (option)
                  {
                      case "info":
                          ServerFunctions.SendData(player, "A trial account lets you try out this game without having to register on the website. You are only able to create 1 character, although you can delete it and start another if you wish. The account will be active for 30 days, after which you can no longer play unless you register. If you haven't registered after 60 days from the start of the trial, the account and character will be deleted.");
                          ServerFunctions.SendData(player, "Some classes will be unavailable and some regions will be inaccessible.");
                          break;
                      case "start":
                          newTrial.player = player;
                          newTrial.account = new Account();
                          ServerFunctions.NewTrialAccount(newTrial, "add");
                          ServerFunctions.SendData(player, "Please set your account name by typing /trial youraccountnamehere");
                          break;
                      default:
                          //if (option == "")
                          //{//there is not option after /trial, or /trial they-typed-garbage
                              ServerFunctions.SendData(player, "Please type /trial start if you would like to start a new trial account or /trial info for more information.");
                         // }
                          break;
                  }
              } else
              {//we have started the new trial account process
                  switch (option)
                  {
                      case "info":
                          ServerFunctions.SendData(player, "A trial account lets you try out this game without having to register on the website. You are only able to create 1 character, although you can delete it and start another if you wish. The account will be active for 30 days, after which you can no longer play unless you register. If you haven't registered after 60 days from the start of the trial, the account and character will be deleted.");
                          ServerFunctions.SendData(player, "Some classes will be unavailable and some regions will be inaccessible.");
                          break;
                      default:
                          if (option != "")
                          {
                              ServerFunctions.SendData(player, option);
                              //if there is something after /trial...
                              if (newTrial.account.AccountName == "moo")//default account name
                              {
                                  //going to assume that whatever is after /trial is their attempt at an account name
                                  bool isGoodAccountName = DBAccess.CheckAccountName(option);
                                  ServerFunctions.SendData(player, isGoodAccountName.ToString());
                              }

                              if (newTrial.account.HashedPassword.Length == 0)
                              {
                                  //going to assume that next, whatever is after /trial is their password, first input
                                  newTrial.account.HashedPassword = option;
                                  ServerFunctions.SendData(player, "Please re-enter your password with /trial yourpasswordhere");
                              } else
                              {
                                  //entering password, second input
                                  if (newTrial.account.HashedPassword != option)
                                  {
                                      //passwords don't match
                                      ServerFunctions.SendData(player, "Your entered passwords don't match. Please enter your password with /trial yourpasswordhere");
                                      newTrial.account.HashedPassword = "";
                                  }

                              }
                          } else
                          {
                              //there is nothing after /trial
                              if (newTrial.account.AccountName == "moo")
                              {
                                  ServerFunctions.SendData(player, "Please type /trial youraccountnamehere to set your account name.");
                              }
                              if (newTrial.account.HashedPassword.Length == 0)
                              {
                                  ServerFunctions.SendData(player, "Please type /trial yourpasswordhere to set your password.");
                              }
                              ServerFunctions.SendData(player, "Please type /trial start if you would like to start a new trial account or /trial info for more information.");
                          }

                          break;


                  }
              }*/


            //      if (message.Length > 1)
            //        option = message[1];//we have a /trial option
            //  if (message.Length > 2)
            //    nameOrPass = message[2]; //we have an account name or password, possibly

            //   if (!isNewTrialStarted && message.Length > 2)
            // {
            //we haven't started the newtrial bit yet and for some reason we have 3 strings in the message. trying to set account or password already?
            //   ServerFunctions.SendData(player, "If you're ready to start a trial account, please type /trial start");
            // return;
            //   }
            //ServerFunctions.SendData(player, option);
            //  if (command.IndexOf(" ") > -1)// || player.AccountType > AccountType.Trial)
            //{
            //we possibly have options to work with
            //String[] splitCommand = command.Split(' ');
            //int numOfOptions = message.Length;

            //       if (player.AccountType > AccountType.Trial)
            //     {
            //       message[1] = "info";
            // }

            /*   switch (option)
               {
                   case "info":
                       ServerFunctions.SendData(player, "A trial account lets you try out this game without having to register on the website. You are only able to create 1 character, although you can delete it and start another if you wish. The account will be active for 30 days, after which you can no longer play unless you register. If you haven't registered after 60 days from the start of the trial, the account and character will be deleted.");
                       ServerFunctions.SendData(player, "Some classes will be unavailable and some regions will be inaccessible.");
                       break;

                   case "start":
                       //this will either start a new trial process in order to get the account name and password to make a new account
                       //or, after confirming the account name and password, create the account in the database
                       if (!isNewTrialStarted)
                       {//this player hasn't started the newTrial process yet
                        //NewTrial tempTrial = new NewTrial();
                           newTrial.player = player;// clientGUID = clientGUID;// client = client;
                                                    //server.newTrialAccounts.Add(newTrial);
                           ServerFunctions.NewTrialAccount(newTrial, "add");
                           //since we just added this player to the newTrial list, obviously the account name and password are both blank to start with
                       }

                       if (newTrial.isAccountName && newTrial.isPassword)
                       {//we've set and confirmed both the account name and password
                           Account newAccount = new Account();
                           newAccount.AccountName = newTrial.account;
                           newAccount.HashedPassword = Config.Hash(newTrial.password);
                           newAccount.AccountType = AccountType.Trial;
                           newAccount.AllowedServers = Config.config.defaultServers;//for now, both servers. when live, this will only be 'live' and would manually set 'test' for specific people, set from config
                           bool validGuid = false;
                           Guid guid = Guid.NewGuid();
                           while (!validGuid)
                           {
                               if (!DBAccess.IsNewAccountGuid(guid))
                               {//if this is not a new guid, try again
                                   guid = Guid.NewGuid();
                               }
                               else
                               {
                                   validGuid = true;
                               }
                           }
                           newAccount.AccountID = guid;
                           DBAccess.CreateAccount(newAccount);
                           player.AccountID = guid;
                           ServerFunctions.NewTrialAccount(newTrial, "remove");
                           //for (int i = 0; i < server.newTrialAccounts.Count; i++)
                           //{
                           //  if (server.newTrialAccounts[i].player == player)
                           //{
                           //  server.newTrialAccounts.RemoveAt(i);
                           //}
                           //}
                       }
                       else
                       {
                           ServerFunctions.SendData(player, "Please make sure to set both your account name and password.");
                           break;
                       }

                       if (!newTrial.isAccountName)
                       {//account name has not been confirmed
                        //  if (newTrial.accountName == "")
                        //  {//account name is default
                           ServerFunctions.SendData(player, "To set your account name, please type /trial account youraccountnamehere");
                           //  } else
                           //  {//account name has been typed in
                           //      server.SendData(player, $"Your current possible account name is {returnedInfo}. If you would like to use");
                           //  }
                       }
                       if (!newTrial.isPassword)
                       {//password has not been confirmed
                           ServerFunctions.SendData(player, "To set your password, please type /trial password yourpasswordhere");
                       }
                       break;
                   case "account":
                       //ServerFunctions.SendData(player, message.Length.ToString());
                       if (message.Length == 3)//   /trial (option) (name or password)
                       {// 1 = trial, 2 = account, 3 = name
                           if (!newTrial.isAccountName)//haven't set the account name yet
                           {
                               //check typed accountname against the database
                               if (message[2] == "accept" && newTrial.account.Length > 0)
                               {
                                   newTrial.isAccountName = true;
                                   ServerFunctions.SendData(player, "Account name set!<br>To set your password, please type /trial password yourpasswordhere");
                                   break;
                               }
                               if (message[2].Length < 8)
                               {
                                   ServerFunctions.SendData(player, "A valid account name needs to be at least 8 characters long. Please choose another.");
                                   break;
                               }
                               ServerFunctions.SendData(player, DBAccess.CheckAccountName(message[2]).ToString());
                               if (DBAccess.CheckAccountName(message[2]))
                               {//account name already exists
                                   ServerFunctions.SendData(player, "That account name is taken. Please choose another.");
                                   //break;
                               }
                               else
                               {//name does not exist yet
                                   ServerFunctions.SendData(player, $"{message[2]} is a valid account name. If you would like to keep this, please type /trial account accept");
                                   ServerFunctions.SendData(player, "If you would like to select a different account name, please type /trial account youraccountnamehere");
                                   newTrial.account = message[2];
                                   //break;
                               }
                           } else
                           {

                           }
                       }
                       else
                       {//'account' by itself
                           if (newTrial.isAccountName)
                           {//they've already set their account name?
                               ServerFunctions.SendData(player, "You have already set your account name! For info, please type /trial info");
                               break;
                           } else
                           {
                               ServerFunctions.SendData(player, "Please choose an account name with /trial account youraccountnamehere");
                               break;
                           }
                       }
                       break;
                   case "password":
                       if (message.Length == 3)
                       {
                           if (!newTrial.isPassword)
                           {//if they've not set their password yet


                               if (newTrial.password.Length == 0)
                               {//first time entering the password, now ask to retype it and see if it matches
                                   newTrial.password = message[2];
                                   ServerFunctions.SendData(player, "Please re-enter /trial password yourpasswordhere");
                               }
                               else
                               {
                                   if (message[2] == newTrial.password)
                                   {
                                       newTrial.isPassword = true;
                                       ServerFunctions.SendData(player, $"Password set for {newTrial.account}! Please make sure to remember your password or save it in the accounts list on the onnaMUD front-end.");
                                       ServerFunctions.SendData(player, "To activate this account, please type /trial start");
                                   }
                                   else
                                   {
                                       ServerFunctions.SendData(player, "Passwords do not match! Please try again.");
                                       newTrial.password = "";
                                       ServerFunctions.SendData(player, "To set your password, please type /trial password yourpasswordhere");
                                   }
                               }
                           }


                       } else
                       {
                           if (newTrial.isPassword)
                           {

                           } else
                           {

                           }
                       }

                       break;
                   default:
                       if (player.AccountID != Guid.Empty)
                       {
                           //this is an active account now
                           ServerFunctions.SendData(player, "active account");
                       } else
                       {
                           //not an active account yet
                           ServerFunctions.SendData(player, "The available options for /trial are: info, start, account or password.");
                       }

                       break;
               }*/

            //  }
            //  else
            //  {
            //  '/trial' by itself
            //ServerFunctions.SendData(player, "To get information about a trial account, please type /trial info<br>To start a trial account, please type /trial start");
            //  }
        }
    }
}

