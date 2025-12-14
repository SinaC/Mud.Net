using Mud.Common;

namespace Mud.POC.Tests.Flags.Stub
{
    [POC.Flags.DefineFlagValues(typeof(string))] // FlagInterfaceType doesn't inherit from Iflags<string>
    public class Flags2Definition3 : POC.Flags.IFlagValues
    {
        public IEnumerable<string> AvailableFlags => ["flag2_5", "flag2_6", "flag2_3"/*duplicate*/, "flag2_4"/*duplicate*/];

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag.ToPascalCase();
    }
}
