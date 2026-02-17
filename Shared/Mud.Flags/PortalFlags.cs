using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class PortalFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IPortalFlags
{
}
