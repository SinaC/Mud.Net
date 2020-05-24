using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IItem : IEntity
    {
        int Level { get; }

        ItemFlags ItemFlags { get; }
        bool RemoveBaseItemFlags(ItemFlags flags);
    }
}
