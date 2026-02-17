using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class OffensiveFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IOffensiveFlags
{
}
