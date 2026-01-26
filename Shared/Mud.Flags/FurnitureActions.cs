using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class FurnitureActions : DataStructures.Flags.Flags, IFurnitureActions
{
    public FurnitureActions(params string[] flags)
        : base(flags)
    {
    }
}
