namespace Mud.Server.Flags.Interfaces;

public interface IFlagValues
{
    IEnumerable<string> AvailableFlags { get; }
    string PrettyPrint(string flag, bool shortDisplay);
}
