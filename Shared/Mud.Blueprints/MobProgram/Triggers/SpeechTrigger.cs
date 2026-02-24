namespace Mud.Blueprints.MobProgram.Triggers;

public class SpeechTrigger : MobProgramTriggerBase
{
    public string Phrase { get; set; } = default!;

    public override string ToString()
        => $"SPEECH phrase: {Phrase}";
}
