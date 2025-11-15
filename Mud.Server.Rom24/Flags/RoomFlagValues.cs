using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

public class RoomFlagValues : FlagValuesBase<string>, IRoomFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Dark",
        "NoMob",
        "Indoors",
        "NoScan",
        "Private",
        "Safe",
        "Solitary",
        "NoRecall",
        "ImpOnly",
        "GodsOnly",
        "NewbiesOnly",
        "Law",
        "NoWhere",
    };

    protected override HashSet<string> HashSet => Flags;

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Log.Default.WriteLine(LogLevels.Error, $"Room flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
