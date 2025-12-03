using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    public class Rom24ShieldFlagValues : FlagValuesBase<string>, IShieldFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "Sanctuary",
            "ProtectEvil",
            "ProtectGood"
        };

        protected override HashSet<string> HashSet => Flags;

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag.ToString();
    }
}
