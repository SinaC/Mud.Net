using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AutoFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IAutoFlags
{
}
