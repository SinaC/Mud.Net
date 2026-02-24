namespace Mud.Server.MobProgram;

public class AndExpr : BoolExpr
{
    public required BoolExpr Left { get; set; }
    public required BoolExpr Right { get; set; }
}
