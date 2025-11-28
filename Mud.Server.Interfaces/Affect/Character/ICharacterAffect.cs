using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterAffect : IAffect
{
    // Attributes

    void Apply(ICharacter character);
}
