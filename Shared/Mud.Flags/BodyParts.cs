using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class BodyParts(params string[] flags) : DataStructures.Flags.Flags(flags), IBodyParts
{
}
