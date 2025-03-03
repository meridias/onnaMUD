//using onnaMUD.Accounts;
using onnaMUD.Characters;
using onnaMUD.Utilities;
using onnaMUD.BaseClasses;
//using onnaMUD.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onnaMUD.MUDServer
{
    public interface ICommand
    {
        string[] Aliases { get; }
        AccountType AllowedUsers { get; }
        bool AllowedInRT { get; }

        int Execute(ServerMain server, Player player, Room room, string[] message);
    }
}
