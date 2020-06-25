using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Server.Rom24.Flags
{
    public class RoomFlagValues : FlagValuesBase<string>, IRoomFlagValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
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
            "Test", // TEST PURPOSE
        };

        protected override HashSet<string> HashSet => Flags;
    }
}
