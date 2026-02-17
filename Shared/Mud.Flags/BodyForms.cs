using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class BodyForms(params string[] flags) : DataStructures.Flags.Flags(flags), IBodyForms
{
}
