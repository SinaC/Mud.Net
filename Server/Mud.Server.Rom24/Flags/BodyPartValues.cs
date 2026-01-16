using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IBodyParts)), Shared]
public class BodyPartValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
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
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
