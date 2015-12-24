using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Commands
{
    public class Impersonate : ICommand
    {
        public CommandFlags Flags
        {
            get { return CommandFlags.OutOfGame; }
        }

        public string Name
        {
            get { return "impersonate"; }
        }

        public string Help { get { return "TODO"; } }

        public bool Execute(IClient actor, string raw, params CommandParameter[] parameters)
        {
            // Usage: impersonate <vnum>: impersonate character matching vnum
            // Usage: impersonate <name>: impersonate character matching name
            // Usage: impersonate <vnum> object|room: impersonate object|room matching vnum
            // Usage: impersonate <name> object|room: impersonate object|room matching vnum
            return true;
        }
    }
}
