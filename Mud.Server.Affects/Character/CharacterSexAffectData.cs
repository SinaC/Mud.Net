using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase), "sex")]
public class CharacterSexAffectData : AffectDataBase
{
    public required Sex Value { get; set; }
}
