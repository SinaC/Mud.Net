using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ItemFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IItemFlags
{
}
