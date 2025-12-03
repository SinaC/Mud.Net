using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IAssistFlagValues)), Shared]
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

    private ILogger<AssistFlagValues> Logger { get; }

    public AssistFlagValues(ILogger<AssistFlagValues> logger)
    {
        Logger = logger;
    }

    protected override HashSet<string> HashSet => Flags;

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Assist flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
