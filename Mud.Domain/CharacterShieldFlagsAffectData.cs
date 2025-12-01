using Mud.Server.Flags.Interfaces;

namespace Mud.Domain;

public class CharacterShieldFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IShieldFlags Modifier { get; set; }
}
