using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IIRVFlags)), Shared]
public class IRVFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Summon",
        "Charm",
        "Magic",
        "Weapon",
        "Bash",
        "Pierce",
        "Slash",
        "Fire",
        "Cold",
        "Lightning",
        "Acid",
        "Poison",
        "Negative",
        "Holy",
        "Energy",
        "Mental",
        "Disease",
        "Drowning",
        "Light",
        "Sound",
        "Wood",
        "Silver",
        "Iron",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
