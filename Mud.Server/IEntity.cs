using System;

namespace Mud.Server
{
    public interface IEntity : IActor, IContainer
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }

        bool Incarnatable { get; }
        IAdmin IncarnatedBy { get; }

        bool ChangeIncarnation(IAdmin admin);

        void OnRemoved();
    }
}
