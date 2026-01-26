using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IExitFlags)), Shared]
public class ExitFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Door",
        "Closed",
        "Locked",
        "Easy",
        "Hard",
        "Hidden",
        "PickProof",
        "NoPass",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
