using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterHitDamageModifierAffect : IAffect
{
    (int modifiedDamage, bool wearOff) ModifyDamage(ICharacter? source, SchoolTypes damageType, int damage);
}
