using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Mud.Server
{
    public interface IClient
    {
        Guid Id { get; }
        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        IEntity Impersonating { get; } // non-null if Client is impersonating an entity (and is then considered as InGame), null is Client not impersonating an entity (and is then considered OutOfGame)

        void GoInGame(IEntity entity);
        void GoOutOfGame();

        bool ProcessCommand(string command);
        List<string> CommandList();
        void OnDisconnected();
    }
}
