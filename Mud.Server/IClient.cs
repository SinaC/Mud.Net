using System;

namespace Mud.Server
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
