using System;

namespace Mud.Server
{
    public interface IEntity : IActor
    {
        Guid Id { get; }
        bool Valid { get; }
        string Name { get; }
        // TODO: keywords: List<string> = Name.Split(' ')
        string DisplayName { get; }
        string Description { get; }

        bool Incarnatable { get; }
        IAdmin IncarnatedBy { get; }

        bool ChangeIncarnation(IAdmin admin);

        void OnRemoved(); // called before removing an item from the game
    }
}
