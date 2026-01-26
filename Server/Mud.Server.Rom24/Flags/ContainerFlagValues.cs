using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IContainerFlags)), Shared]
public class ContainerFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Closed",
        "Locked",
        "PickProof",
        "NoClose",
        "NoLock",
        "Easy",
        "Hard"
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
