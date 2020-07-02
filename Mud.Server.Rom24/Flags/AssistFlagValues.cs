using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Server.Rom24.Flags
{
    public class AssistFlagValues : FlagValuesBase<string>, IAssistFlagValues
    {
        private static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "All",
            "Align",
            "Race",
            "Players",
            "Guard",
            "Vnum",
        };

        protected override HashSet<string> HashSet => Flags;

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            Log.Default.WriteLine(LogLevels.Error, $"Assist flags '{string.Join(",", values)}' not found in {GetType().FullName}");
        }
    }
}
