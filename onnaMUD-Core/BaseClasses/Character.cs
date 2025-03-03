using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using onnaMUD.Characters;

namespace onnaMUD.BaseClasses
{
    //this is the base for ALL characters, player AND NPC ?
    public class Character : Thing
    {
        public Character()
        {
            ThingType = ThingType.Character;
        }

        //[Key]
//        public Guid Id { get; set; }//guid ID for this character, when character is created and added to database
        public Guid AccountID { get; set; }//account id for the player who has this character, .Empty for npc?
        //public int CharacterID { get; set; }
//        public string CharName { get; set; } = "";
        public string Server { get; set; }//which server this character is made on
        public Guid CurrentRoom { get; set; }
        public Player? Player { get; set; } = null;//is this character being controlled by a PC?
        
        public Races Race { get; set; }
        public Gender Gender { get; set; }


    }

    public enum Races
    {
        Human = 0,
        Elf = 1,
        Dwarf = 2,
        Halfling = 3
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 3
    }
}
