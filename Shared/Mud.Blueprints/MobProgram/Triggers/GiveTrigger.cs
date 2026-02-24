namespace Mud.Blueprints.MobProgram.Triggers;

public class GiveTrigger : MobProgramTriggerBase
{
    public int? ObjectId { get; set; }
    public string? ObjectName { get; set; }
    public bool IsAll { get; set; }
}
