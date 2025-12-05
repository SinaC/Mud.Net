using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using System.Reflection;

namespace Mud.Server.Ability;

[Export(typeof(IAbilityManager)), Shared]
public class AbilityManager : IAbilityManager
{
    private ILogger<AbilityManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private Dictionary<string, IAbilityInfo> AbilityByName { get; } // TODO: trie to optimize Search ?
    private Dictionary<Type, IAbilityInfo[]> AbilitiesByExecutionType { get; }
    private Dictionary<WeaponTypes, IAbilityInfo> WeaponAbilityByWeaponType { get; }

    public AbilityManager(ILogger<AbilityManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        AbilityByName = new Dictionary<string, IAbilityInfo>(StringComparer.InvariantCultureIgnoreCase);
        WeaponAbilityByWeaponType = [];
        // Get abilities
        var iWeaponPassiveType = typeof(IWeaponPassive);
        var iAbilityType = typeof(IAbility);
        foreach (var abilityType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && iAbilityType.IsAssignableFrom(t))))
        {
            AbilityInfo abilityInfo = new (logger, abilityType);
            if (AbilityByName.ContainsKey(abilityInfo.Name))
                Logger.LogError("Duplicate ability {abilityInfoName}", abilityInfo.Name);
            else
                AbilityByName.Add(abilityInfo.Name, abilityInfo);

            // if weapon passive, add to weapon ability by weapon type cache
            if (iWeaponPassiveType.IsAssignableFrom(abilityType))
            {
                var weaponAttribute = abilityType.GetCustomAttribute<WeaponAttribute>();
                if (weaponAttribute != null)
                {
                    foreach (var weaponTypeName in weaponAttribute.WeaponTypes)
                    {
                        if (!Enum.TryParse<WeaponTypes>(weaponTypeName, out var weaponType))
                            Logger.LogError("Weapon passive ability {abilityInfoName} refers to an unknown weapon type {weaponType}", abilityInfo.Name, weaponTypeName);
                        else
                        {
                            if (WeaponAbilityByWeaponType.ContainsKey(weaponType))
                                Logger.LogError("Duplicate weapon passive ability {weaponType}", weaponType);
                            else
                                WeaponAbilityByWeaponType.Add(weaponType, abilityInfo);
                        }
                    }
                }
            }
        }
        //
        AbilitiesByExecutionType = []; // will be filled at each call to AbilitiesByExecutionType
    }

    #region IAbilityManager

    public IEnumerable<IAbilityInfo> Abilities => AbilityByName.Values;

    public IAbilityInfo? this[string abilityName]
    {
        get
        {
            if (!AbilityByName.TryGetValue(abilityName, out var abilityInfo))
                return null;
            return abilityInfo;
        }
    }

    public IAbilityInfo? this[WeaponTypes weaponType]
    {
        get
        {
            if (!WeaponAbilityByWeaponType.TryGetValue(weaponType, out var abilityInfo))
                return null;
            return abilityInfo;
        }
    }

    public IEnumerable<IAbilityInfo> SearchAbilities<TAbility>()
            where TAbility : class, IAbility
    {
        Type tAbilityType = typeof(TAbility);
        var abilities = Abilities.Where(x => tAbilityType.IsAssignableFrom(x.AbilityExecutionType)).ToArray();
        return abilities;
    }

    public IEnumerable<IAbilityInfo> SearchAbilitiesByExecutionType<TAbility>()
        where TAbility : class, IAbility
    {
        Type tAbilityType = typeof(TAbility);
        // Check in cache first
        if (AbilitiesByExecutionType.TryGetValue(tAbilityType, out var abilities))
            return abilities;
        // Not found in cache, compute and put in cache
        abilities = Abilities.Where(x => tAbilityType.IsAssignableFrom(x.AbilityExecutionType)).ToArray();
        AbilitiesByExecutionType.Add(tAbilityType, abilities);
        return abilities;
    }

    public IAbilityInfo? Search(string pattern, AbilityTypes type)
    {
        // TODO: use Trie ? or save in cache
        return Abilities.FirstOrDefault(x => x.Type == type && StringCompareHelpers.StringStartsWith(x.Name, pattern));
    }

    public IAbilityInfo? Search(ICommandParameter parameter)
        => Abilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));

    public TAbility? CreateInstance<TAbility>(string abilityName)
        where TAbility : class, IAbility
    {
        var abilityInfo = this[abilityName];
        if (abilityInfo == null)
        {
            Logger.LogError("Ability {abilityName} doesn't exist.", abilityName);
            return default;
        }
        return CreateInstance<TAbility>(abilityInfo, abilityName);
    }

    public TAbility? CreateInstance<TAbility>(IAbilityInfo abilityInfo)
        where TAbility : class, IAbility
        => CreateInstance<TAbility>(abilityInfo, abilityInfo.Name);

    #endregion

    private TAbility? CreateInstance<TAbility>(IAbilityInfo abilityInfo, string abilityName)
         where TAbility : class, IAbility
    {
        var ability = ServiceProvider.GetService(abilityInfo.AbilityExecutionType);
        if (ability == null)
        {
            Logger.LogError("Ability {abilityName} not found in DependencyContainer.", abilityName);
            return default;
        }
        if (ability is not TAbility instance)
        {
            Logger.LogError("Ability {abilityName} cannot be created or is not {expectedAbilityType}.", abilityName, typeof(TAbility).Name);
            return default;
        }
        return instance;
    }
}
