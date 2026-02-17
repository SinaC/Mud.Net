using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ActFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IActFlags
{
}
