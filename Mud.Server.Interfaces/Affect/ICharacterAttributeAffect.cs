using Mud.Domain;

namespace Mud.Server.Interfaces.Affect
{
    public interface ICharacterAttributeAffect : ICharacterAffect
    {
        CharacterAttributeAffectLocations Location { get; set; }
        AffectOperators Operator { get; set; } // Or and Nor cannot be used
        int Modifier { get; set; }
    }
}
