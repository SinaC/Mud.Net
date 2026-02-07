using Mud.Domain;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterDamageModifierAffect : IAffect
{
    DamageModifierAffectResult ModifyDamage(ICharacter? source, ICharacter victim, SchoolTypes damageType, DamageSources damageSource, int damage);
}
