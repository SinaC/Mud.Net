using Mud.Server.Interfaces.Item;

namespace Mud.Server.Effects.Interfaces;

public interface IWeaponEffectManager
{
    int Count { get; }

    TWeaponEffect? CreateInstance<TWeaponEffect>(string weaponEffectName)
        where TWeaponEffect : class, IWeaponEffect;

    IEnumerable<string> WeaponEffectsByType<TWeaponEffect>(IItemWeapon weapon)
        where TWeaponEffect : IWeaponEffect;
}
