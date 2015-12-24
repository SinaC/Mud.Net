using System;
using System.Collections.Generic;

namespace Mud.Server
{
    public interface IClient
    {
        Guid Id { get; }
        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        bool Impersonating { get; } // true if Client is impersonating a mob/obj/room and is then considered as InGame, false is Client is OutOfGame

        bool ProcessCommand(string command);
        List<string> CommandList();
        void OnDisconnected();
    }
}
