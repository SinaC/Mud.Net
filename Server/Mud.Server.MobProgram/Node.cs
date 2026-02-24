using Mud.Server.MobProgram.Interfaces;

namespace Mud.Server.MobProgram;

public abstract class Node : INode
{
    public required string Line { get; set; }
}
