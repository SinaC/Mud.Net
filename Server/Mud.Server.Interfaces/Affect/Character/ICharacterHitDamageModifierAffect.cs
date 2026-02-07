using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterHitDamageModifierAffect : IAffect
{
    (int modifiedDamage, bool wornOff) ModifyDamage(ICharacter? source, SchoolTypes damageType, int damage);
}
