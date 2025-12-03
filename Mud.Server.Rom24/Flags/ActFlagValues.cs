using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IActFlagValues)), Shared]
public class ActFlagValues : FlagValuesBase<string>, IActFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
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
    };

    private ILogger<ActFlagValues> Logger { get; }

    public ActFlagValues(ILogger<ActFlagValues> logger)
    {
        Logger = logger;
    }

    protected override HashSet<string> HashSet => Flags;

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Act flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
