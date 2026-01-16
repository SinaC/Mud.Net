using Mud.Common.Attributes;
using Mud.Flags;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IAssistFlags)), Shared]
public class AssistFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "All",
        "Align",
        "Race",
        "Players",
        "Guard",
        "Vnum",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
