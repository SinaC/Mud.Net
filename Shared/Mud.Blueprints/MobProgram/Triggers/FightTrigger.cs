namespace Mud.Blueprints.MobProgram.Triggers;

public class FightTrigger : MobProgramTriggerBase
{
    public int Percentage { get; set; }

    public override string ToString()
        => $"FIGHT percentage: {Percentage}%";
}
