namespace Mud.Blueprints.MobProgram.Triggers;

public class KillTrigger : MobProgramTriggerBase
{
    public int Percentage { get; set; }

    public override string ToString()
        => $"KILL percentage: {Percentage}";
}
