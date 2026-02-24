namespace Mud.Blueprints.MobProgram.Triggers;

public class DeathTrigger : MobProgramTriggerBase
{
    public int Percentage { get; set; }

    public override string ToString()
        => $"DEATH percentage: {Percentage}%";
}
