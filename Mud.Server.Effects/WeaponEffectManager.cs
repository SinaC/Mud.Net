using Mud.Container;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using System.Reflection;

namespace Mud.Server.Effects;

public class WeaponEffectManager : IWeaponEffectManager
{
    private readonly Dictionary<string, Type> _weaponEffectsByWeaponFlag;

    public WeaponEffectManager(IAssemblyHelper assemblyHelper)
    {
        var iWeaponEffectType = typeof(IWeaponEffect);
        _weaponEffectsByWeaponFlag = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iWeaponEffectType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<WeaponEffectAttribute>()! })
            .Where(x => x.attribute != null)
            .ToDictionary(x => x.attribute.WeaponFlagName, x => x.executionType);
    }

    public int Count => _weaponEffectsByWeaponFlag.Count;

    public TWeaponEffect? CreateInstance<TWeaponEffect>(string weaponEffectName)
        where TWeaponEffect : class, IWeaponEffect
    {
        if (!_weaponEffectsByWeaponFlag.TryGetValue(weaponEffectName, out var weaponEffect))
        {
            Log.Default.WriteLine(LogLevels.Warning, "WeaponEffectManager: weapon flag {0} not found.", weaponEffectName);
            return null;
        }

        var tWeaponEffectType = typeof(TWeaponEffect);
        if (!tWeaponEffectType.IsAssignableFrom(weaponEffect))
        {
            Log.Default.WriteLine(LogLevels.Error, "WeaponEffectManager: weapon effect type {0} is not of type {1}.", weaponEffect.FullName ?? "???", tWeaponEffectType.FullName ?? "???");
            return null;
        }

        if (DependencyContainer.Current.GetRegistration(weaponEffect, false) == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} not found in DependencyContainer.", weaponEffect.FullName ?? "???");
            return null;
        }

        if (DependencyContainer.Current.GetInstance(weaponEffect) is not TWeaponEffect instance)
        {
            Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} cannot be created or is not of type {1}", weaponEffect.FullName ?? "???", tWeaponEffectType.FullName ?? "???");
            return null;
        }

        return instance;
    }

    public IEnumerable<string> WeaponEffectsByType<TWeaponEffect>(IItemWeapon weapon)
        where TWeaponEffect : IWeaponEffect
    {
        Type tWeaponEffectType = typeof(TWeaponEffect);
        foreach (string weaponFlag in weapon.WeaponFlags.Items)
        {
            if (!_weaponEffectsByWeaponFlag.TryGetValue(weaponFlag, out var weaponEffect))
                Log.Default.WriteLine(LogLevels.Warning, "WeaponEffectManager: weapon flag {0} has no associated effect.", weaponFlag);
            else
            {
                if (tWeaponEffectType.IsAssignableFrom(weaponEffect)) // if this weapon flag is of the right type, returns it
                    yield return weaponFlag;
            }
        }
    }
}
