using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterRegenModifierAffect : IAffect
{
    int Modifier { get; }
    AffectOperators Operator { get; }
}
