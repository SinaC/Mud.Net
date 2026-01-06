namespace Mud.Domain.SerializationData;

public class AdminData
{
    public required AdminLevels AdminLevel { get; set; }
    public required WiznetFlags WiznetFlags { get; set; }
}
