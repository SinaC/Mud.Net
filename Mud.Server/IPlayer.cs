using System;

namespace Mud.Server
{
    public interface IPlayer : IActor
    {
        Guid Id { get; }
        string Name { get; }

        ICharacter Impersonating { get; }

        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        void OnDisconnected();
    }
}
