namespace Mud.Blueprints.MobProgram.Triggers;

public class GreetTrigger : MobProgramTriggerBase
{
    public int Percentage { get; set; }
    public bool IsAll { get; set; }

    public override string ToString()
        => $"GREET percentage: {Percentage}% isAll: {IsAll}";
}
