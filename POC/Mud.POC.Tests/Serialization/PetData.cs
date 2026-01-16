namespace Mud.POC.Tests.Serialization;

[POC.Serialization.Discriminator]
[POC.Serialization.Polymorphism(typeof(CharacterData))]
public class PetData : CharacterData
{
    public int BlueprintId { get; set; }
}
