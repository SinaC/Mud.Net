using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Server.Rom24.Flags
{
    public class IRVFlagValues : FlagValuesBase<string>, IIRVFlagValues
    {
        private static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Summon",
            "Charm",
            "Magic",
            "Weapon",
            "Bash",
            "Pierce",
            "Slash",
            "Fire",
            "Cold",
            "Lightning",
            "Acid",
            "Poison",
            "Negative",
            "Holy",
            "Energy",
            "Mental",
            "Disease",
            "Drowning",
            "Light",
            "Sound",
            "Wood",
            "Silver",
            "Iron",
        };

        protected override HashSet<string> HashSet => Flags;

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            Log.Default.WriteLine(LogLevels.Error, $"IRV flags '{string.Join(",", values)}' not found in {GetType().FullName}");
        }
    }
}
