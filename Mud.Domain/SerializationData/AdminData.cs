using Mud.Common.Attributes;

namespace Mud.Domain.SerializationData;

[JsonBaseType(typeof(PlayerData), "admin")]
public class AdminData : PlayerData
{
    public required AdminLevels AdminLevel { get; set; }
    public required WiznetFlags WiznetFlags { get; set; }

    // TODO: extra fields
}
