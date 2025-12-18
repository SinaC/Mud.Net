using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Server.Domain;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase), "shieldFlags")]
public class CharacterShieldFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required string Modifier { get; set; }
}
