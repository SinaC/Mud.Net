using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IBodyPartValues)), Shared]
public class BodyPartValues : FlagValuesBase<string>, IBodyPartValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Head",
        "Arms",
        "Legs",
        "Heart",
        "Brains",
        "Guts",
        "Hands",
        "Feet",
        "Fingers",
        "Ear",
        "Eye",
        "LongTongue",
        "Eyestalks",
        "Tentacles",
        "Fins",
        "Wings",
        "Tail",
        "Body",
        "Claws",
        "Fangs",
        "Horns",
        "Scales",
        "Tusks",
    };

    protected override HashSet<string> HashSet => Flags;

    public BodyPartValues(ILogger<BodyPartValues> logger)
        : base(logger)
    {
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Body part flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
