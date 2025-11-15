using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

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

    protected override HashSet<string> HashSet => Flags;

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Log.Default.WriteLine(LogLevels.Error, $"Offensive flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
