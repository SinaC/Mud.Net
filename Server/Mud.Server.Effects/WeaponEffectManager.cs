using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Item;
using System.Reflection;

namespace Mud.Server.Effects;

[Export(typeof(IWeaponEffectManager)), Shared]
public class WeaponEffectManager : IWeaponEffectManager
{
    private ILogger<WeaponEffectManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private Dictionary<string, Type> WeaponEffectsByWeaponFlag { get; }

    public WeaponEffectManager(ILogger<WeaponEffectManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        var iWeaponEffectType = typeof(IWeaponEffect);
        WeaponEffectsByWeaponFlag = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iWeaponEffectType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<WeaponEffectAttribute>()! })
            .Where(x => x.attribute != null)
            .ToDictionary(x => x.attribute.WeaponFlagName, x => x.executionType);
    }

    public int Count => WeaponEffectsByWeaponFlag.Count;

    public TWeaponEffect? CreateInstance<TWeaponEffect>(string weaponEffectName)
        where TWeaponEffect : class, IWeaponEffect
    {
        if (!WeaponEffectsByWeaponFlag.TryGetValue(weaponEffectName, out var weaponEffectType))
        {
            Logger.LogWarning("WeaponEffectManager: weapon flag {weaponEffectName} not found.", weaponEffectName);
            return null;
        }

        var tWeaponEffectType = typeof(TWeaponEffect);
        if (!tWeaponEffectType.IsAssignableFrom(weaponEffectType))
        {
            Logger.LogError("WeaponEffectManager: weapon effect type {weaponEffectType} is not of type {expectedWeaponEffectType}.", weaponEffectType.FullName ?? "???", tWeaponEffectType.FullName ?? "???");
            return null;
        }

        var weaponEffect = ServiceProvider.GetRequiredService(weaponEffectType);
        if (weaponEffect is not TWeaponEffect instance)
        {
            Logger.LogError("WeaponEffectManager: weapon effect {weaponEffectType} cannot be created or is not of type {expectedWeaponEffectType}", weaponEffectType.FullName ?? "???", tWeaponEffectType.FullName ?? "???");
            return null;
        }

        return instance;
    }

    public IEnumerable<string> WeaponEffectsByType<TWeaponEffect>(IItemWeapon weapon)
        where TWeaponEffect : IWeaponEffect
    {
        var tWeaponEffectType = typeof(TWeaponEffect);
        foreach (var weaponFlag in weapon.WeaponFlags.Values)
        {
            if (WeaponEffectsByWeaponFlag.TryGetValue(weaponFlag, out var weaponEffect))
            {
                if (tWeaponEffectType.IsAssignableFrom(weaponEffect)) // if this weapon flag is of the right type, returns it
                    yield return weaponFlag;
            }
        }
    }
}
