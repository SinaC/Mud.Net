namespace Mud.POC.Flags
{
    public class FlagDefinition
    {
        public string Flag { get; }
        public IFlagValues DefineFlagValues { get; }

        public FlagDefinition(string flag, IFlagValues defineFlagValues)
        {
            Flag = flag;
            DefineFlagValues = defineFlagValues;
        }
    }
}
