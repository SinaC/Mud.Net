using Mud.Domain;
using Mud.Server.Domain;

namespace Mud.POC.Tests.Serialization;

[POC.Serialization.AffectDataDiscriminator]
[POC.Serialization.Discriminator]
[POC.Serialization.Polymorphism(typeof(AffectDataBase))]
public class CharacterAttributeAffectData : AffectDataBase
{
    public AffectOperators Operator { get; set; } // Or and Nor cannot be used
    public CharacterAttributeAffectLocations Location { get; set; }
    public int Modifier { get; set; }
}
