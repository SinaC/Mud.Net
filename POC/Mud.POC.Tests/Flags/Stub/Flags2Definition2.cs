using Mud.Common;

namespace Mud.POC.Tests.Flags.Stub
{
    // missing attribute
    public class Flags2Definition2 : POC.Flags.IFlagValues
    {
        public IEnumerable<string> AvailableFlags => ["flag2_5", "flag2_6", "flag2_3"/*duplicate*/, "flag2_4"/*duplicate*/];

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag.ToPascalCase();
    }
}
