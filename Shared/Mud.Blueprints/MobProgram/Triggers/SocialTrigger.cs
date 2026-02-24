namespace Mud.Blueprints.MobProgram.Triggers;

public class SocialTrigger : MobProgramTriggerBase
{
    public string Social { get; set; } = default!;

    public override string ToString()
        => $"SOCIAL social: {Social}";
}
