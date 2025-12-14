using Mud.Common;

namespace Mud.POC.Tests.Flags.Stub
{
    [POC.Flags.DefineFlagValues(typeof(IFlags2))]
    public class Flags2Definition1 : POC.Flags.IFlagValues
    {
        public IEnumerable<string> AvailableFlags => ["flag2_1", "flag2_2", "flag2_3", "flag2_4"];

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag.ToPascalCase();
    }
}
