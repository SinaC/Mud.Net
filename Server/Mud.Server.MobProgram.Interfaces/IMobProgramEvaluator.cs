namespace Mud.Server.MobProgram.Interfaces;

public interface IMobProgramEvaluator
{
    bool Evaluate(IMobProgram mobProgram, IMobProgramExecutionContext ctx);
}
