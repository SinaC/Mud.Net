using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Effects.Interfaces;

public interface IDamageModifierWeaponEffect : IWeaponEffect
{
    int DamageModifier(ICharacter holder, ICharacter victim, IItemWeapon weapon, int learned, int baseDamage);
}
