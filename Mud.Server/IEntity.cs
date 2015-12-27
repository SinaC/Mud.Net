using System;

namespace Mud.Server
{
    public interface IEntity : IActor
    {
        Guid Id { get; }
        string Name { get; }
        string Keyword { get; }
        string Description { get; }

        bool Incarnatable { get; }
        IAdmin IncarnatedBy { get; }

        bool ChangeIncarnation(IAdmin admin);

        void OnRemoved();
    }
}
