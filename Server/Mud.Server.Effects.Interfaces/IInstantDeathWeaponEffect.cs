using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Effects.Interfaces;

public interface IInstantDeathWeaponEffect : IWeaponEffect
{
    bool Trigger(ICharacter holder, ICharacter victim, IItemWeapon weapon, SchoolTypes damageType);
}
