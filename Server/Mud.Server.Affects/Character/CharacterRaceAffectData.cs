using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase), "race")]
public class CharacterRaceAffectData : AffectDataBase
{
    public required string Race { get; set; }
}
