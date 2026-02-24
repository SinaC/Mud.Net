namespace Mud.Server.MobProgram;

public class OrExpr : BoolExpr
{
    public required BoolExpr Left { get; set; }
    public required BoolExpr Right { get; set; }
}
