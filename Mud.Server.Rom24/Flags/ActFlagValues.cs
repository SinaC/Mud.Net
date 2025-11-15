using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

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

    protected override HashSet<string> HashSet => Flags;

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Log.Default.WriteLine(LogLevels.Error, $"Act flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
