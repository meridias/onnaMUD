using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.BaseClasses
{
    public class CustomVerb
    {
        public string Alias { get; set; } = "";//the command used for this custom verb on this object/item
        public string SelfSee { get; set; } = "";//what the player sees
        public string OtherSee { get; set; } = "";//what others see
        public VerbEffects Effect { get; set; }//any effects that this custom verb cause (touch race statue at CharGen changes race, etc)
    }

    public enum VerbEffects
    {
        none = 0,
        setRace = 1

    }
}
