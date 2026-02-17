using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AuraFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IAuraFlags
{
}
