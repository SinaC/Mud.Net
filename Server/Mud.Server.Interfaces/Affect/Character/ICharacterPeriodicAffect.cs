using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterPeriodicAffect : IAffect
{
    void Apply(IAura aura, ICharacter character);
}
