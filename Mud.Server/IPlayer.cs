using System;

namespace Mud.Server
{
    // IPlayer can impersonate
    public interface IPlayer : IActor
    {
        Guid Id { get; }
        Guid AvatarId { get; } // Specific ICharacter that can be impersonated by non-admin player
        string Name { get; }
        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        IEntity Impersonating { get; } // non-null if Player is impersonating an entity (and is then considered as InGame), null is Player not impersonating an entity (and is then considered OutOfGame)

        void StartImpersonating(IEntity target);
        void StopImpersonating();

        void OnDisconnected();
    }
}
