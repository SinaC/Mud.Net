namespace Mud.POC.Tests.Flags.Stub
{
    [POC.Flags.DefineFlagValues(typeof(IFlags1))]
    public class Flags1Definition1 : POC.Flags.IFlagValues
    {
        public IEnumerable<string> AvailableFlags => ["flag1_1", "flag1_2", "flag1_3"];

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag switch
            {
                "flag1_1" => "(Flag11)",
                "flag1_2" => "(Flag12)",
                _ => string.Empty // we don't want to display the other flags
            };
    }
}
