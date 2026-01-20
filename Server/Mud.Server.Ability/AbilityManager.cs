using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using System.Reflection;

namespace Mud.Server.Ability;

[Export(typeof(IAbilityManager)), Shared]
public class AbilityManager : IAbilityManager
{
    private ILogger<AbilityManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private Dictionary<string, IAbilityDefinition> AbilityByName { get; }
    private Dictionary<Type, IAbilityDefinition> AbilityByType { get; }
    private Dictionary<WeaponTypes, IAbilityDefinition> WeaponAbilityByWeaponType { get; }
    private Dictionary<AbilityTypes, IReadOnlyTrie<IAbilityDefinition>> AbilityDefinitionTrieByAbilityTypes { get; }
    private Dictionary<Type, IAbilityDefinition[]> AbilitiesByExecutionType { get; } // created on-the-fly when needed

    public AbilityManager(ILogger<AbilityManager> logger, IServiceProvider serviceProvider, IGuardGenerator guardGenerator, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        // create ability definitions collection
        var abilityDefinitions = new List<IAbilityDefinition>();
        var iAbilityType = typeof(IAbility);
        foreach (var abilityType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && iAbilityType.IsAssignableFrom(t))))
        {
            var characterGuards = guardGenerator.GenerateCharacterGuards(abilityType);
            var abilityDefinition = new AbilityDefinition(abilityType, characterGuards);
            if (abilityDefinitions.Any(x => StringCompareHelpers.StringEquals(x.Name, abilityDefinition.Name)))
                Logger.LogError("Duplicate ability {abilityDefinitionName}", abilityDefinition.Name);
            else
                abilityDefinitions.Add(abilityDefinition);
        }
        // create optimized structures
        // by name
        AbilityByName = abilityDefinitions.ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);
        // by type
        AbilityByType = abilityDefinitions.ToDictionary(x => x.AbilityExecutionType);
        // by weapon type
        WeaponAbilityByWeaponType = [];
        var iWeaponPassiveType = typeof(IWeaponPassive);
        foreach (var weaponAbilityDefinition in abilityDefinitions.Where(x => iWeaponPassiveType.IsAssignableFrom(x.AbilityExecutionType)))
        {
            var weaponAttribute = weaponAbilityDefinition.AbilityExecutionType.GetCustomAttribute<WeaponAttribute>();
            if (weaponAttribute != null)
            {
                foreach (var weaponTypeName in weaponAttribute.WeaponTypes)
                {
                    if (!Enum.TryParse<WeaponTypes>(weaponTypeName, out var weaponType))
                        Logger.LogError("Weapon passive ability {abilityDefinitionName} refers to an unknown weapon type {weaponType}", weaponAbilityDefinition.Name, weaponTypeName);
                    else
                    {
                        if (!WeaponAbilityByWeaponType.TryAdd(weaponType, weaponAbilityDefinition))
                            Logger.LogError("Duplicate weapon passive ability {weaponType}", weaponType);
                    }
                }
            }
        }
        // by ability type then trie
        AbilityDefinitionTrieByAbilityTypes = [];
        foreach (var groupedByAbilityType in abilityDefinitions.GroupBy(x => x.Type))
        {
            var trieEntries = groupedByAbilityType.Select(x => new TrieEntry<IAbilityDefinition>(x.Name.ToLowerInvariant(), x));
            var trie = new Trie<IAbilityDefinition>(trieEntries);
            AbilityDefinitionTrieByAbilityTypes.Add(groupedByAbilityType.Key, trie);
        }
        // by execution type, will be created on-the-fly when needed
        AbilitiesByExecutionType = [];
    }

    #region IAbilityManager

    public IEnumerable<IAbilityDefinition> Abilities => AbilityByName.Values;

    public IAbilityDefinition? this[string abilityName]
        => AbilityByName.GetValueOrDefault(abilityName);

    public IAbilityDefinition? this[Type abilityType]
        => AbilityByType.GetValueOrDefault(abilityType);

    public IAbilityDefinition? this[WeaponTypes weaponType]
        => WeaponAbilityByWeaponType.GetValueOrDefault(weaponType);

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

    public IAbilityDefinition? Get(string abilityName, AbilityTypes type)
    {
        if (!AbilityDefinitionTrieByAbilityTypes.TryGetValue(type, out var trie))
        {
            Logger.LogError("AbilityType {type} doesn't exist", type);
            return null;
        }
        return trie.GetValueOrDefault(abilityName);
    }

    public IEnumerable<IAbilityDefinition> Search(ICommandParameter parameter, AbilityTypes type)
    {
        if (!AbilityDefinitionTrieByAbilityTypes.TryGetValue(type, out var trie))
        {
            Logger.LogError("AbilityType {type} doesn't exist", type);
            return [];
        }
        return trie.GetByPrefix(parameter.Value.ToLowerInvariant()).Select(x => x.Value).ToArray();
    }

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
