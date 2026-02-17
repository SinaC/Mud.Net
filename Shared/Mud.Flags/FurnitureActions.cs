using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class FurnitureActions(params string[] flags) : DataStructures.Flags.Flags(flags), IFurnitureActions
{
}
