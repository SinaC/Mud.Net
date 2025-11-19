using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

public class AssistFlagValues : FlagValuesBase<string>, IAssistFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "All",
        "Align",
        "Race",
        "Players",
        "Guard",
        "Vnum",
    };

    protected override HashSet<string> HashSet => Flags;

    public AssistFlagValues(ILogger<AssistFlagValues> logger)
        : base(logger)
    {
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError($"Assist flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
