using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AreaFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IAreaFlags
{
}
