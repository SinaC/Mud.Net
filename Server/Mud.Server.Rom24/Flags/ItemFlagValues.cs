using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IItemFlags)), Shared]
public class ItemFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Glowing",
        "Humming",
        "Dark", // not used
        "Lock", // not used
        "Evil", // used but has no impact
        "Invis",
        "Magic",
        "NoDrop", // Cannot be dropped once in inventory (cannot be put in container) [can be uncursed]
        "Bless",
        "AntiGood",
        "AntiEvil",
        "AntiNeutral",
        "NoRemove", // Cannot be removed once equipped [can be uncursed]
        "Inventory",
        "NoPurge",
        "RotDeath", // Disappear when holder dies
        "VisibleDeath", // Visible when holder dies
        "NonMetal",
        "NoLocate",
        "MeltOnDrop", // Melt when dropped
        "HadTimer", // not used
        "SellExtract", // no haggle and can be sold without any reduction
        "BurnProof",
        "NoUncurse",
        "NoSacrifice",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
