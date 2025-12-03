using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    public class Rom24RoomFlagValues : FlagValuesBase<string>, IRoomFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
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
