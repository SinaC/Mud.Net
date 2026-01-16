using Mud.Common.Attributes;
using Mud.Flags;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IActFlags)), Shared]
public class ActFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Sentinel",
        "Scavenger",
        "StayArea",
        "Aggressive",
        "Wimpy",
        "Pet",
        "Undead",
        "NoAlign",
        "NoPurge",
        "Outdoors",
        "Indoors",
        "UpdateAlways",
        "Train",
        "IsHealer",
        "Gain",
        "Practice",
        "Aware",
        "Warrior",
        "Thief",
        "Cleric",
        "Mage",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
