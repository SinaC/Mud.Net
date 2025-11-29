using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IIRVFlagValues)), Shared]
public class IRVFlagValues : FlagValuesBase<string>, IIRVFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
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
    };

    protected override HashSet<string> HashSet => Flags;

    public IRVFlagValues(ILogger<IRVFlagValues> logger)
        : base(logger)
    {
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("IRV flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
