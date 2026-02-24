using Mud.Server.MobProgram.Interfaces;

namespace Mud.Server.MobProgram;

public class IfNode : Node
{
    public required BoolExpr Condition { get; set; }

    public List<INode> TrueBranch { get; } = [];
    public List<INode> FalseBranch { get; } = [];
}

