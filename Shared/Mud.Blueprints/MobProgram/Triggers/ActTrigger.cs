namespace Mud.Blueprints.MobProgram.Triggers;

public class ActTrigger : MobProgramTriggerBase
{
    public string Phrase { get; set; } = default!;

    public override string ToString()
        => $"ACT phrase: {Phrase}";
}
