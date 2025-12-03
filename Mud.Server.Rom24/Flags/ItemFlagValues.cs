using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IItemFlagValues)), Shared]
public class ItemFlagValues : FlagValuesBase<string>, IItemFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Glowing",
        "Humming",
        "Dark",
        "Lock",
        "Evil",
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
        "HadTimer",
        "SellExtract",
        "BurnProof",
        "NoUncurse",
        "NoSacrifice",
    };

    private ILogger<ItemFlagValues> Logger { get; }

    public ItemFlagValues(ILogger<ItemFlagValues> logger)
    {
        Logger = logger;
    }

    protected override HashSet<string> HashSet => Flags;

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Item flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
