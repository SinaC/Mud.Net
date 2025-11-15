using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect;

public interface ICharacterAffect : IAffect
{
    // Attributes

    void Apply(ICharacter character);
}
