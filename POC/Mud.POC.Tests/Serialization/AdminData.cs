using Mud.Domain;
using Mud.POC.Serialization;

namespace Mud.POC.Tests.Serialization;

[Polymorphism(typeof(PlayerData))]
public class AdminData : PlayerData
{
    public AdminLevels AdminLevel { get; set; }
    public WiznetFlags WiznetFlags { get; set; }

    // TODO: extra fields
}
