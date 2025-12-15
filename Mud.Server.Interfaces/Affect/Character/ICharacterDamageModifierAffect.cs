using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterDamageModifierAffect : IAffect
{
    int ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, int damage);
}
