using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    public class Rom24BodyPartValues : FlagValuesBase<string>, IBodyPartValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
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
    }
}
