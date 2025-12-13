using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Affects.Character;

[JsonPolymorphism(typeof(AffectDataBase), "characterFlags")]
public class CharacterFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required ICharacterFlags Modifier { get; set; }
}
