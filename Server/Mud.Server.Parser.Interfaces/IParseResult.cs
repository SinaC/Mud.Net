namespace Mud.Server.Parser.Interfaces;

public interface IParseResult
{
    public string Command { get; }
    public string RawParameters { get; }
    public ICommandParameter[] Parameters { get; }
    public bool ForceOutOfGame { get; }

    void ModifyCommand(string newCommand);
}
