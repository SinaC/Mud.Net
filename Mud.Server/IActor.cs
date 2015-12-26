using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server
{
    public interface IActor
    {
        bool ProcessCommand(string commandLine);
        bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters);
        void Send(string message);
    }
}
