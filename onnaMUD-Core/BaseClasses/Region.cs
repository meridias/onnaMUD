using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.BaseClasses
{
    public class Region
    {
        public Guid Id { get; set; }
        public bool IsInstanced { get; set; } = false;//does this region spawn a new instance of it when somebody enters?
        public List<Room> Rooms { get; set; } = new();
        public bool ShowName { get; set; } = false;//whether or not to show the region name next to the room name in 'look'
    }
}
