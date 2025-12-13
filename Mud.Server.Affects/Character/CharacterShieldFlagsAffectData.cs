using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Affects.Character;

[JsonPolymorphism(typeof(AffectDataBase), "shieldFlags")]
public class CharacterShieldFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IShieldFlags Modifier { get; set; }
}
