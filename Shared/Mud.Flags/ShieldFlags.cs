using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ShieldFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IShieldFlags
{
}
