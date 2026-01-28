using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase))]
public class CharacterRegenModifierAffectData : AffectDataBase
{
    public required int Modifier { get; set; } = 1;
    public required AffectOperators Operator { get; set; } = AffectOperators.Multiply;
}
