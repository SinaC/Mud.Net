using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ImmortalModes(params string[] flags) : DataStructures.Flags.Flags(flags), IImmortalModes
{
}
