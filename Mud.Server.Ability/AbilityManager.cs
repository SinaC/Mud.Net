using Mud.Common;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Ability;

public class AbilityManager : IAbilityManager
{
    private IServiceProvider ServiceProvider { get; }
    private Dictionary<string, IAbilityInfo> AbilityByName { get; } // TODO: trie to optimize Search ?
    private Dictionary<Type, IAbilityInfo[]> AbilitiesByExecutionType { get; }

    public AbilityManager(IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        ServiceProvider = serviceProvider;

        AbilityByName = new Dictionary<string, IAbilityInfo>(StringComparer.InvariantCultureIgnoreCase);
        // Get abilities
        Type iAbility = typeof(IAbility);
        foreach (var abilityType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iAbility.IsAssignableFrom(t))))
        {
            AbilityInfo abilityInfo = new (abilityType);
            if (AbilityByName.ContainsKey(abilityInfo.Name))
                Log.Default.WriteLine(LogLevels.Error, "Duplicate ability {0}", abilityInfo.Name);
            else
                AbilityByName.Add(abilityInfo.Name, abilityInfo);
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

    public IAbilityInfo? Search(ICommandParameter parameter) => Abilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));

    public TAbility? CreateInstance<TAbility>(string abilityName)
        where TAbility : class, IAbility
    {
        var abilityInfo = this[abilityName];
        if (abilityInfo == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "Ability {0} doesn't exist.", abilityName);
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
            Log.Default.WriteLine(LogLevels.Error, "Ability {0} not found in DependencyContainer.", abilityName);
            return default;
        }
        if (ability is not TAbility instance)
        {
            Log.Default.WriteLine(LogLevels.Error, "Ability {0} cannot be created or is not {1}.", abilityName, typeof(TAbility).Name);
            return default;
        }
        return instance;
    }
}
