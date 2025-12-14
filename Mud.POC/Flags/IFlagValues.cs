namespace Mud.POC.Flags
{
    public interface IFlagValues
    {
        IEnumerable<string> AvailableFlags { get; }
        string PrettyPrint(string flag, bool shortDisplay);
    }
}
