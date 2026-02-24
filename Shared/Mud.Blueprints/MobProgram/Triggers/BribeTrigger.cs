namespace Mud.Blueprints.MobProgram.Triggers;

public class BribeTrigger : MobProgramTriggerBase
{
    public int Amount { get; set; }

    public override string ToString()
        => $"BRIBE amount: {Amount}";
}
