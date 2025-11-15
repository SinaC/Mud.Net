namespace Mud.Domain;

public class AdminData : PlayerData
{
    public required AdminLevels AdminLevel { get; set; }
    public required WiznetFlags WiznetFlags { get; set; }

    // TODO: extra fields
}
