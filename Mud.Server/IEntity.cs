using System;

namespace Mud.Server
{
    public interface IEntity : IActor
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        bool Impersonable { get; }

        IEntity Controlling { get; } // controlling another entity
        IPlayer ImpersonatedBy { get; } // impersonated by an IPlayer
        IEntity ControlledBy { get; } // controlled by another IEntity

        void StartControlling(IEntity slave);
        void StopControlling();
        void StartBeingImpersonated(IPlayer by);
        void StopBeingImpersonated();
        void StartBeingControlled(IEntity master);
        void StopBeingControlled();
    }
}
