using Mud.Server.Flags.Interfaces;

namespace Mud.Domain.SerializationData;

public class CharacterFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required ICharacterFlags Modifier { get; set; }
}
