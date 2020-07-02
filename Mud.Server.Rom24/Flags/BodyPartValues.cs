using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Server.Rom24.Flags
{
    public class BodyPartValues : FlagValuesBase<string>, IBodyPartValues
    {
        private static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
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

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            Log.Default.WriteLine(LogLevels.Error, $"Body parts flags '{string.Join(",", values)}' not found in {GetType().FullName}");
        }
    }
}
