using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class WiznetFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IWiznetFlags
{
}
