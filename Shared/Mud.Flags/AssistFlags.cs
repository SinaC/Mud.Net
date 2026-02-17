using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AssistFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IAssistFlags
{
}
