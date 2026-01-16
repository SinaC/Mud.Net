namespace Mud.POC.Tests.Serialization;

[POC.Serialization.Discriminator]
[POC.Serialization.Polymorphism(typeof(CharacterData))]
public class PlayableCharacterData : CharacterData
{
    public DateTime CreationTime { get; set; }

    public PetData[] Pets { get; set; } = null!;
}
