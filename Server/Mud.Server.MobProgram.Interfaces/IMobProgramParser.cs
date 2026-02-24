namespace Mud.Server.MobProgram.Interfaces;

public interface IMobProgramParser
{
    IEnumerable<INode> Parse(string program);
}
