using onnaMUD.Characters;
using onnaMUD.Database;
using onnaMUD.MUDServer;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.Temps
{
    public class CharManager
    {
        public Player source { get; set; }//player doing the editing
        public Player target { get; set; }//player getting edited
        public bool editSelf = false;//this is true when either an admin is editing themselves, or at character creation
        public bool isCharCreation;

        public List<int> races = new List<int>();

        public CharManager(Player editor, Player editee)//, bool newChar = false)
        {
            source = editor;
            target = editee;
            //isCharCreation = newChar;

            if (source == target)
            {
                editSelf = true;
            }
 //           if (editor.Id == Guid.Empty)
 //           {
 //               isCharCreation = true;
 //           }

            //don't bother with admin check here. we check for admin if they try to do 'race' anyway
            //if (editor.AccountType == AccountType.Admin)
            // {
            //if the person editing is admin
            //races = new List<Races>();
            races = Enum.GetValues(typeof(Races)).Cast<int>().ToList();

            //}

        }

        public void Choose(Player player, string[] choices)
        {
            //string[] split = choices.Split(' ');
            string option = "";
            int choice = -1;

            if (choices.Length == 1)
            {
                //no option
                ServerFunctions.SendData(player, "Please type CHOOSE (option) or CHOOSE (name) to choose your character name.");
                ServerFunctions.SendData(player, $"The available options to choose from are: {(player.AccountType == AccountType.Admin ? "RACE" : "")}");
                return;
            }
            if (choices.Length > 1)
            {
                option = choices[1];
            }
            if (choices.Length > 2)
            {
                if (!Int32.TryParse(choices[2], out choice))
                {
                    ServerFunctions.SendData(player, "Invalid selection! Please enter the number for the option you want.");
                    return;
                }
            }


            switch(option.ToLower())
            {
                case "race":
                    //not normally an option, but if we're an admin we can change this
                    if (player.AccountType < AccountType.Admin)
                        return;

                    if (choice != -1)
                    {
                        player.character.Race = (Races)races[choice - 1];
                        ServerFunctions.SendData(player, $"You are now a {player.character.Race}.");
                        break;
                    }
                    

                    for (int i = 0; i < races.Count; i++)
                    {
                        ServerFunctions.SendData(player, $"{i + 1}) <link=\"choose race {i+1}\">{(Races)i}</link>");
                    }
                    break;

                default:
                    //since there is something in 'option' and we're here, assuming it's CHOOSE (name)
                    if (DB.DBAccess.IsUsedCharName(option))
                    {
                        //this character name is used
                        ServerFunctions.SendData(player, $"{option} is not a valid character name. Please choose another.");
                    } else
                    {
                        ServerFunctions.SendData(player, "Name chosed!");
                        player.character.Name = option;
                    }
                    break;

            }

        }




    }
}
