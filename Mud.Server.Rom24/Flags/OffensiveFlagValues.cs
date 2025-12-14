using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IOffensiveFlags)), Shared]
public class OffensiveFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "AreaAttack",
        "Backstab",
        "Bash",
        "Berserk",
        "Disarm",
        "Dodge",
        "Fade",
        "Fast",
        "Kick",
        "DirtKick",
        "Parry",
        "Rescue",
        "Tail",
        "Trip",
        "Crush",
        "Bite",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
