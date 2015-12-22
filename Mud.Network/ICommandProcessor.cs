using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Network
{
    public interface ICommandProcessor
    {
        void ProcessCommand(IClient client, string command);
    }
}
