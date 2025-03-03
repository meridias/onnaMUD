//using onnaMUD.Accounts;
using onnaMUD.Characters;
using onnaMUD.Database;
using onnaMUD.Settings;
using onnaMUD.Utilities;
using onnaMUD.MUDServer;
using onnaMUD.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.Temps
{
    public class NewAccount// : ITemp
    {
        private string Account { get; set; } = "";
        private string Password { get; set; } = "";

        public NewAccount(Player player)
        {
            //ServerFunctions.SendData(player, "054", "open");
            ServerFunctions.SendData(player, "060", "newaccount::open");
        }

        public void Check(Player player, string name, string password1, string password2)//  string message)
        {
            //Temps.SetNewTrialInfo(player, "", "");
            //clear these just in case the player somehow sent new info after old ones got verified already? dunno. we'll see how it goes.
            Account = "";
            Password = "";
            //do verification checks
            if (name.Length < 8)
            {
                ServerFunctions.SendData(player, "Your account name needs to be at least 8 characters long. Please try again.");
                //break;
                return;
            }
            bool isUsedAccountName = DB.DBAccess.CheckAccountName(name);

            if (isUsedAccountName)
            {
                //name found, can't use
                ServerFunctions.SendData(player, $"{name} is not a valid new account name. Please choose another one.");
                //break;
                return;

                //possible name not found, can use this one, no
                //ServerFunctions.SendData(player, $"{splitMessage[2]} is a valid new account name.");
            }
            // else
            // {
            //name found, can't use
            //   ServerFunctions.SendData(player, $"{splitMessage[2]} is not a valid new account name. Please choose another one.");
            // break;
            // }

            if (password1 != password2)
            {
                //if the two entered passwords don't match
                ServerFunctions.SendData(player, "Your entered passwords don't match. Please re-enter them.");
                //break;
                return;
            }
            //   else
            // {
            //   ServerFunctions.SendData(player, "Your entered passwords match!.");
            // }
            //if we got to this point, account name is acceptable and passwords match
            //also, close the new account window. if they want to pick something else they can send trail redo
            //Temps.SetNewTrialInfo(player, splitMessage[2], splitMessage[3]);
            Account = name;
            Password = password1;
            ServerFunctions.SendData(player, "054", "close");
            ServerFunctions.SendData(player, "You have picked a valid account name and your passwords match. If you would like to use these as your login, please type TRIAL ACCEPT or if you want to redo these, type TRIAL REDO");
            //break;
        }

        public void Clear()
        {
            Account = "";
            Password = "";
        }

        public void Create(Player player)
        {
            if (Account.Length > 0 && Password.Length > 0)
            {
                //if the name and password are both longer than 0, meaning we've put something in there, then we're good?
                Account account = new Account();
                account.Id = DB.DBAccess.GetNewGuid<Account>(DB.Collections.Account);
                account.AccountName = Account;
                account.HashedPassword = Config.Hash(Password);
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
                ServerFunctions.SendData(player, "052", $"account::{account.AccountName}");
                DB.DBAccess.Save(account, DB.Collections.Account);
                //player.tempScript = null;
                player.AccountID = account.Id;
            }
        }

    }
}
