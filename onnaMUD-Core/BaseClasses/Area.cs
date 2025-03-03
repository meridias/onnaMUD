using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.BaseClasses
{
    public class Area
    {
        public Guid Id { get; set; }
        public Guid SubArea { get; set; }//if this area is a sub-area of another area, put that area's guid here, if not, leave empty. (building interior area is sub-area to main city area, etc...)
        public bool IsInstanced { get; set; } = false;//does this region spawn a new instance of it when somebody enters?
        public Guid SameLocationAs { get; set; }//if this isn't Guid.Empty, use the location of the referenced roomID (interior area of a building will have the same location as the room the entrance to that building is in)
        [BsonIgnore]//, JsonIgnore]
        public List<Room> Rooms { get; set; } = new();//we'll add rooms to this list during server startup
//        public bool ShowName { get; set; } = false;//whether or not to show the region name next to the room name in 'look', nah just have it in room name if we're gonna do it
    }
}
