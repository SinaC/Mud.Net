namespace Mud.Blueprints.MobProgram.Triggers;

public class DelayTrigger : MobProgramTriggerBase
{
    public int Percentage { get; set; }
    public override string ToString()
        => $"DELAY percentage: {Percentage}%";
}
