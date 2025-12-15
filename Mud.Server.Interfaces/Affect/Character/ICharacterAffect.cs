using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterAffect : IAffect
{
    void Apply(ICharacter character);
}
