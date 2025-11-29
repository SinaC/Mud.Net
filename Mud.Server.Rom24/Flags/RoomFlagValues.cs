using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IRoomFlagValues)), Shared]
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

    public RoomFlagValues(ILogger<RoomFlagValues> logger)
        : base(logger)
    {
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Room flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
