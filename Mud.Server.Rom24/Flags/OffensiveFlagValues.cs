using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IOffensiveFlagValues)), Shared]
public class OffensiveFlagValues : FlagValuesBase<string>, IOffensiveFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
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
    };

    private ILogger<OffensiveFlagValues> Logger { get; }

    public OffensiveFlagValues(ILogger<OffensiveFlagValues> logger)
    {
        Logger = logger;
    }

    protected override HashSet<string> HashSet => Flags;


    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Offensive flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
