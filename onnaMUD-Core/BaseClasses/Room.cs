using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.BaseClasses
{
    //Rooms are part of an Area: (1 room up to however many, specifies instances rooms, hunting areas for mod spawns/movement, building interior)
    //Areas can have/be sub-Areas (building interior is a sub-area to the area that the building is in)

    //Provinces: whole main geographical location (country, island chain, etc...)

    //longitude/latitude based room node coordinates? it would help with calculating range on some things...

    public class Room
    {
        //[Key]
        public Guid Id { get; set; }// guid ID for this room
        public string Name { get; set; } = "New Room";//name of room that shows at top of description when looking or going into new room
        public string Description { get; set; } = "blah";//main room description
        public Guid Area { get; set; }//this room is part of what area?
        public Guid Region { get; set; }//which region is this room a part of? try to have every room be a part of SOME region, even if it's just 1 room or a building interior
        public RoomType RoomType { get; set; }
        public Directions Directions { get; set; } = new();
        public List<RoomObject> Objects { get; set; } = new();//all the permanent objects in this room


        [BsonIgnore]//, JsonIgnore]
        public List<Thing> StuffInRoom { get; set; } = new();//all the NPCs, Players, Items and Objects in the room for the 'You also see', or is it 'Also here:'? or is that just players?

        //public List<Thing> Stuff { get; set; } = new();//?
    }

    public class Directions
    {
        public Exit? North { get; set; }
        public Exit? NorthEast { get; set; }
        public Exit? East { get; set; }
        public Exit? SouthEast { get; set; }
        public Exit? South { get; set; }
        public Exit? SouthWest { get; set; }
        public Exit? West { get; set; }
        public Exit? NorthWest { get; set; }
        public Exit? Up { get; set; }
        public Exit? Down { get; set; }
        public Exit? Out { get; set; }

    }

    public class Exit
    {
        public Guid ExitTo { get; set; }
        public string ExitCommand { get; set; } = "go";//default is go for most non-cardinal directions (go portal, go door), other options are climb
        public List<ExitStep> Steps { get; set; } = new();
    }

    public class ExitStep
    {
        public int WaitTimer { get; set; } = 0;//time in milliseconds to wait before showing output from this step and/or moving on (timer runs first so for first step, no timer)
        public string SoloOutput { get; set; } = "";
        public string GroupLeader { get; set; } = "";
        public string GroupFollow { get; set; } = "";

    }

    public enum RoomType
    {
        Standard = 0,
        CharCreate = 1

    }
}
