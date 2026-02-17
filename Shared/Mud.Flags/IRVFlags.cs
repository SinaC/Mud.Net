using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class IRVFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IIRVFlags
{
}
