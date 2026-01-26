using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase))]
public class CharacterAggroModifierAffectData : AffectDataBase
{
    public required int MultiplierInPercent { get; set; } = 100;
}
