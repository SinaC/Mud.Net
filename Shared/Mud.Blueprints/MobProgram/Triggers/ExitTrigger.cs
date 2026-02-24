using Mud.Domain;

namespace Mud.Blueprints.MobProgram.Triggers;

public class ExitTrigger : MobProgramTriggerBase
{
    public ExitDirections Direction { get; set; }
    public bool IsAll { get; set; }

    public override string ToString()
        => $"EXIT dir: {Direction} isAll: {IsAll}";
}
