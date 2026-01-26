using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IPortalFlags)), Shared]
public class PortalFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Closed",
        "Locked",
        "PickProof",
        "NoClose",
        "NoLock",
        "NoCurse",
        "GoWith",
        "Buggy",
        "Random",
        "Easy",
        "Hard",
    ];
    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
