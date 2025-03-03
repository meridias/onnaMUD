using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.MUDServer
{
    public class Server
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "default";
        public string Type { get; set; } = "game";//not gonna need this, only have game type servers which handle their own logging-in
        public bool Debug { get; set; } = false;//does this server start in debug mode
        public bool IsDefault { get; set; } = false;//is this a server that gets added to new accounts so they automatically have access? not needed for login server, just game
        public Guid FirstRoom { get; set; }//this is the room that new characters start in, character creator room
        public int ServerPort { get; set; } = 1;
        public bool AutoStart { get; set; } = false;
    }
}
