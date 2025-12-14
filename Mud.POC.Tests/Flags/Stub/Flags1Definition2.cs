using Mud.Common;

namespace Mud.POC.Tests.Flags.Stub
{
    [POC.Flags.DefineFlagValues(typeof(IFlags1))]
    public class Flags1Definition2 : POC.Flags.IFlagValues
    {
        public IEnumerable<string> AvailableFlags => ["flag1_1"/*duplicate*/, "flag1_4", "flag1_5", "flag1_6"];

        public string PrettyPrint(string flag, bool shortDisplay)
            => "(" + flag.ToPascalCase() + ")";
    }
}
