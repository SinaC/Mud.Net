namespace Mud.Blueprints.MobProgram.Triggers;

public class EntryTrigger : MobProgramTriggerBase
{
    public int Percentage { get; set; }

    public override string ToString()
        => $"ENTRY percentage: {Percentage}%";
}
