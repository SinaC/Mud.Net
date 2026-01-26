using Mud.Domain;
using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterAttributeAffect : ICharacterAffect
{
    CharacterAttributeAffectLocations Location { get; }
    AffectOperators Operator { get; } // Or and Nor cannot be used
    int Modifier { get; }
}
