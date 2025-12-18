using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase), "size")]
public class CharacterSizeAffectData : AffectDataBase
{
    public required Sizes Value { get; set; }
}
