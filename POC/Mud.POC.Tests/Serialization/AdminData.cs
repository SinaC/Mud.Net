using Mud.Domain;
using Mud.POC.Serialization;

namespace Mud.POC.Tests.Serialization;

[Polymorphism(typeof(PlayerData))]
public class AdminData : PlayerData
{
    public AdminLevels AdminLevel { get; set; }
    public string WiznetFlags { get; set; } = default!;

    // TODO: extra fields
}
