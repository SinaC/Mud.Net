using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(PlayerData), "admin")]
public class AdminData : PlayerData
{
    public required AdminLevels AdminLevel { get; set; }
    public required WiznetFlags WiznetFlags { get; set; }

    // TODO: extra fields
}
