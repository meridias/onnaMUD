using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.BaseClasses
{
    public class NPC
    {//this is obviously for NPC characters
     //       public bool IsUnique { get; set; }//if only 1 is allowed per server to be active//all actual 'NPC' type characters will be unique, all the non-uniques will be classified as mobs
        public Character? character { get; set; } = null;
        public bool SpawnOnStartup { get; set; } = false;//does this npc get spawned at server startup (shopkeepers, guildmasters, other uniques)

    }
}
