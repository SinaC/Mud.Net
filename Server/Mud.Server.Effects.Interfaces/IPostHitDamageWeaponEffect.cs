using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Effects.Interfaces;

public interface IPostHitDamageWeaponEffect : IWeaponEffect
{
    bool Apply(ICharacter holder, ICharacter victim, IItemWeapon weapon);
}
