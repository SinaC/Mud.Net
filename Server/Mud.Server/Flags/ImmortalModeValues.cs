using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Flags;

[FlagValues(typeof(IFlagValues), typeof(IImmortalModes)), Shared]
public class ImmortalModeValues : IFlagValues
{
    private static readonly string[] Flags =
    [
        "NoDeath",
        "Holylight",
        "Infinite",
        "PassThru",
        "AlwaysSafe",
        "Omniscient"
    ];

    // TODO
    //UberMode = NoDeath | Holylight | Infinite | PassThru | AlwaysSafe | Omniscient,

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
