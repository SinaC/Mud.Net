using Mud.Server.Interfaces.Item;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.Effect
{
    public interface IWeaponEffectManager
    {
        int Count { get; }

        TWeaponEffect CreateInstance<TWeaponEffect>(string weaponEffectName)
            where TWeaponEffect : class, IWeaponEffect;

        IEnumerable<string> WeaponEffectsByType<TWeaponEffect>(IItemWeapon weapon)
            where TWeaponEffect : IWeaponEffect;
    }
}
