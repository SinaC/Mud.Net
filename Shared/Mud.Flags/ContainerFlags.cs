using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ContainerFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IContainerFlags
{
}
