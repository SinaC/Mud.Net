using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Options;

namespace Mud.Server.Ability;

[Export(typeof(IOmniscienceManager)), Shared]
public class OmniscienceManager : IOmniscienceManager
{
    private int MaxLevel { get; }
 
    private Dictionary<string, IAbilityLearned> AbilityLearnedByAbilityName { get; }

    public OmniscienceManager(IAbilityManager abilityManager, IClassManager classManager, IOptions<WorldOptions> worldOptions)
    {
        MaxLevel = worldOptions.Value.MaxLevel;

        AbilityLearnedByAbilityName = BuildAbilityLearnedByAbilityName(abilityManager, classManager);
    }

    public IAbilityLearned? this[string abilityName]
        => AbilityLearnedByAbilityName.GetValueOrDefault(abilityName);

    public IEnumerable<IAbilityLearned> LearnedAbilities => AbilityLearnedByAbilityName.Values;

    private Dictionary<string, IAbilityLearned> BuildAbilityLearnedByAbilityName(IAbilityManager abilityManager, IClassManager classManager)
    {
        var abilityLearnedByAbilityName = new Dictionary<string, IAbilityLearned>();
        foreach (var abilityDefinition in abilityManager.Abilities)
        {
            // search ability usage in any class, if not found create an artificial one
            var abilityUsage = classManager.Classes.SelectMany(x => x.AvailableAbilities).FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, abilityDefinition.Name)) ?? new AbilityUsage(abilityDefinition.Name, MaxLevel, [], 1, 100, abilityDefinition);
            // create ability learned from ability usage
            var abilityLearned = new AbilityLearned(abilityUsage);
            abilityLearned.SetLearned(100);

            abilityLearnedByAbilityName.Add(abilityDefinition.Name, abilityLearned);
        }
        return abilityLearnedByAbilityName;
    }
}
