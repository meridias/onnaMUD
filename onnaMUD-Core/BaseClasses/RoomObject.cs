using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.BaseClasses
{
    public class RoomObject : Thing//permanent objects in rooms (buildings, statues, bridges, etc.)
    {
        public RoomObject()
        {
            ThingType = ThingType.RoomObject;
        }

        //[Key]
//        public Guid Id { get; set; }// guid ID for this object
//        public string Name { get; set; } = "NewObject";//internal name for this, in case we need to do searches for it to help narrow down a list, also maybe this is the generic description of it?
        public bool IsInteractable { get; set; } = false;//if it's possible to do anything with it (basically it doesn't exist in the room)
        public bool IsVisible { get; set; } = false;//if this is shown in the 'you also see:' list along with other players and stuff on the ground
        public bool IsContainer { get; set; } = false;
        public Exit? Exit { get; set; }//not all objects are exits, but all 'other' exits are objects (portals, bridges, doors, etc...)
        //public bool IsExit { get; set; } = false;//if this object can be used as an exit (go building, go bridge, go portal, etc.), this won't be needed for the OtherExits list since everything in there is an exit already
        //public string ExitCommand { get; set; } = "go";//command alias to 'use' this exit, if we have one (go is default, climb possibly?)
        public string BaseObject { get; set; } = "";//base thing to interact with (statue, door, building, etc.)
        public string Descriptors { get; set; } = "";//  ',' delimited list of descriptors for this object (red,mahonagy) etc. so you can do things to red door, etc.
        public Storage In { get; set; }
        public Storage On { get; set; }
        public List<CustomVerb> Verbs { get; set; } = new();//basically overrides for verb behavior on this object/item: custom eat/touch/swap/etc...

    }

    public enum BaseObjectType
    {
        statue,
        door,
        bridge

    }

    public class Storage
    {

        //public List<>  this will be Item once I actually get that class created
    }

}
