namespace Mud.POC.Tests.Serialization;

[POC.Serialization.AffectDataDiscriminator(nameof(CharacterAdditionalHitAffectData))]
[POC.Serialization.Discriminator]
[POC.Serialization.Polymorphism(typeof(AffectDataBase))]
public class CharacterAdditionalHitAffectData : AffectDataBase
{
    public int HitCount { get; set; }
}
