namespace Mud.Blueprints.MobProgram.Triggers;

public class GiveTrigger : MobProgramTriggerBase
{
    public int? ObjectId { get; set; }
    public string? ObjectName { get; set; }
    public bool IsAll { get; set; }

    public override string ToString()
        => $"GIVE id: {ObjectId} name: {ObjectName} isAll: {IsAll}";
}
