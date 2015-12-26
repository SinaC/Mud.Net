using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server
{
    public class CommandParameter
    {
        public static readonly CommandParameter Empty = new CommandParameter();
        public static readonly CommandParameter Invalid = new CommandParameter();

        public int Count { get; set; }
        public string Value { get; set; }
    }
}
