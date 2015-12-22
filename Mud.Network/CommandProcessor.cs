using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Logger;

namespace Mud.Network
{
    public class CommandProcessor : ICommandProcessor
    {
        public void ProcessCommand(IClient client, string command)
        {
            // TODO
            Log.Default.WriteLine(LogLevels.Debug, "Processing " + command + " from client " + client.Id);
        }
    }
}
