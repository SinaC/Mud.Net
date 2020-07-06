using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Server.Rom24.Flags
{
    public class WeaponFlagValues : FlagValuesBase<string>, IWeaponFlagValues
    {
        private static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Flaming",
            "Frost",
            "Vampiric",
            "Sharp",
            "Vorpal",
            "TwoHands",
            "Shocking",
            "Poison",
        };

        protected override HashSet<string> HashSet => Flags;

        public override string PrettyPrint(string flag, bool shortDisplay)
        {
            switch (flag)
            {
                case "Flaming": return "%R%(Flaming)%x%";
                case "Frost": return "%C%(Frost)%x%";
                case "Vampiric": return "%D%(Vampiric)%x%";
                case "Sharp": return "%W%(Sharp)%x%";
                case "Vorpal": return "%M%(Vorpal)%x%";
                case "TwoHands": return "%W%(Two-handed)%x%";
                case "Shocking": return "%Y%(Sparkling)%x%";
                case "Poison": return "%G%(Envenomed)%x%";
                default: return base.PrettyPrint(flag, shortDisplay);
            }
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            Log.Default.WriteLine(LogLevels.Error, $"Weapon flags '{string.Join(",", values)}' not found in {GetType().FullName}");
        }
    }
}
