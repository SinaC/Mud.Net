using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Network
{
    public interface IClient
    {
        Guid Id { get; }
        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        void ProcessCommand(string command);
        void OnDisconnected();
    }
}
