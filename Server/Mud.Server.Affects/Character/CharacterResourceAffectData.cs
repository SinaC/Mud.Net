using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase), "characterResource")]
public class CharacterResourceAffectData : AffectDataBase
{
    public AffectOperators Operator { get; set; } // Or and Nor cannot be used
    public ResourceKinds Location { get; set; }
    public int Modifier { get; set; }
}
