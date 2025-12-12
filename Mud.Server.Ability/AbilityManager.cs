using Microsoft.Extensions.DependencyInjection;
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
    private Dictionary<string, IAbilityDefinition> AbilityByName { get; } // TODO: trie to optimize Search ?
    private Dictionary<Type, IAbilityDefinition[]> AbilitiesByExecutionType { get; }
    private Dictionary<WeaponTypes, IAbilityDefinition> WeaponAbilityByWeaponType { get; }

    public AbilityManager(ILogger<AbilityManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        AbilityByName = new Dictionary<string, IAbilityDefinition>(StringComparer.InvariantCultureIgnoreCase);
        WeaponAbilityByWeaponType = [];
        // Get abilities
        var iWeaponPassiveType = typeof(IWeaponPassive);
        var iAbilityType = typeof(IAbility);
        foreach (var abilityType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && iAbilityType.IsAssignableFrom(t))))
        {
            AbilityDefinition abilityDefinition = new (abilityType);
            if (AbilityByName.ContainsKey(abilityDefinition.Name))
                Logger.LogError("Duplicate ability {abilityDefinitionName}", abilityDefinition.Name);
            else
                AbilityByName.Add(abilityDefinition.Name, abilityDefinition);

            // if weapon passive, add to weapon ability by weapon type cache
            if (iWeaponPassiveType.IsAssignableFrom(abilityType))
            {
                var weaponAttribute = abilityType.GetCustomAttribute<WeaponAttribute>();
                if (weaponAttribute != null)
                {
                    foreach (var weaponTypeName in weaponAttribute.WeaponTypes)
                    {
                        if (!Enum.TryParse<WeaponTypes>(weaponTypeName, out var weaponType))
                            Logger.LogError("Weapon passive ability {abilityDefinitionName} refers to an unknown weapon type {weaponType}", abilityDefinition.Name, weaponTypeName);
                        else
                        {
                            if (WeaponAbilityByWeaponType.ContainsKey(weaponType))
                                Logger.LogError("Duplicate weapon passive ability {weaponType}", weaponType);
                            else
                                WeaponAbilityByWeaponType.Add(weaponType, abilityDefinition);
                        }
                    }
                }
            }
        }
        //
        AbilitiesByExecutionType = []; // will be filled at each call to AbilitiesByExecutionType
    }

    #region IAbilityManager

    public IEnumerable<IAbilityDefinition> Abilities => AbilityByName.Values;

    public IAbilityDefinition? this[string abilityName]
    {
        get
        {
            if (!AbilityByName.TryGetValue(abilityName, out var abilityDefinition))
                return null;
            return abilityDefinition;
        }
    }

    public IAbilityDefinition? this[WeaponTypes weaponType]
    {
        get
        {
            if (!WeaponAbilityByWeaponType.TryGetValue(weaponType, out var abilityDefinition))
                return null;
            return abilityDefinition;
        }
    }

    public IEnumerable<IAbilityDefinition> SearchAbilities<TAbility>()
            where TAbility : class, IAbility
    {
        Type tAbilityType = typeof(TAbility);
        var abilities = Abilities.Where(x => tAbilityType.IsAssignableFrom(x.AbilityExecutionType)).ToArray();
        return abilities;
    }

    public IEnumerable<IAbilityDefinition> SearchAbilitiesByExecutionType<TAbility>()
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

    public IAbilityDefinition? Search(string pattern, AbilityTypes type)
    {
        // TODO: use Trie ? or save in cache
        return Abilities.FirstOrDefault(x => x.Type == type && StringCompareHelpers.StringStartsWith(x.Name, pattern));
    }

    public IAbilityDefinition? Search(ICommandParameter parameter)
        => Abilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));

    public TAbility? CreateInstance<TAbility>(string abilityName)
        where TAbility : class, IAbility
    {
        var abilityDefinition = this[abilityName];
        if (abilityDefinition == null)
        {
            Logger.LogError("Ability {abilityName} doesn't exist.", abilityName);
            return default;
        }
        return CreateInstance<TAbility>(abilityDefinition, abilityName);
    }

    public TAbility? CreateInstance<TAbility>(IAbilityDefinition abilityDefinition)
        where TAbility : class, IAbility
        => CreateInstance<TAbility>(abilityDefinition, abilityDefinition.Name);

    #endregion

    private TAbility? CreateInstance<TAbility>(IAbilityDefinition abilityDefinition, string abilityName)
         where TAbility : class, IAbility
    {
        var ability = ServiceProvider.GetRequiredService(abilityDefinition.AbilityExecutionType);
        if (ability is not TAbility instance)
        {
            Logger.LogError("Ability {abilityName} cannot be created or is not {expectedAbilityType}.", abilityName, typeof(TAbility).Name);
            return default;
        }
        return instance;
    }
}
