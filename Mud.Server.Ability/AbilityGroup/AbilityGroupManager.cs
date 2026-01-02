using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.GameAction;
using System.Reflection;

namespace Mud.Server.Ability.AbilityGroup
{
    [Export(typeof(IAbilityGroupManager)), Shared]
    public class AbilityGroupManager : IAbilityGroupManager
    {
        private ILogger<AbilityGroupManager> Logger { get; }
        private IAbilityManager AbilityManager { get; }

        private readonly Dictionary<string, IAbilityGroupDefinition> _abilityGroupByName; // TODO: trie to optimize Search ?

        public AbilityGroupManager(ILogger<AbilityGroupManager> logger, IAbilityManager abilityManager, IEnumerable<IAbilityGroup> abilityGroups)
        {
            Logger = logger;
            AbilityManager = abilityManager;

            _abilityGroupByName = new Dictionary<string, IAbilityGroupDefinition>(StringComparer.InvariantCultureIgnoreCase);
            GenerateAbilityGroupDefinitions(abilityGroups);
        }

        public IEnumerable<IAbilityGroupDefinition> AbilityGroups => _abilityGroupByName.Values;

        public IAbilityGroupDefinition? this[string abilityGroupName]
            => _abilityGroupByName.GetValueOrDefault(abilityGroupName);

        public IAbilityGroupDefinition? Search(ICommandParameter parameter)
            => AbilityGroups.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));

        private void GenerateAbilityGroupDefinitions(IEnumerable<IAbilityGroup> abilityGroups)
        {
            foreach (var abilityGroup in abilityGroups)
            {
                if (!_abilityGroupByName.ContainsKey(abilityGroup.Name))
                {
                    GenerateAbilityGroupDefinition(abilityGroup, abilityGroups);
                }
            }
        }

        private IAbilityGroupDefinition GenerateAbilityGroupDefinition(IAbilityGroup abilityGroup, IEnumerable<IAbilityGroup> abilityGroups)
        {
            // TODO: handle cycle
            // TODO: what if a group is defined twice with a different ability composition ?
            if (_abilityGroupByName.TryGetValue(abilityGroup.Name, out var existingAbilityGroupDefinition))
                return existingAbilityGroupDefinition;
            var subAbilityGroupDefinitions = new List<IAbilityGroupDefinition>();
            foreach (var subGroupName in abilityGroup.AbilityGroups)
            {
                if (_abilityGroupByName.TryGetValue(subGroupName, out var subAbilityGroupDefinition))
                    subAbilityGroupDefinitions.Add(subAbilityGroupDefinition);
                else
                {
                    // search sub ability group name in ability group types found in DI
                    var subAbilityGroup = abilityGroups.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, subGroupName));
                    if (subAbilityGroup == null)
                        Logger.LogError("Ability group [{subGroupName}] defined in ability group [{groupName}] doesn't exist", subGroupName, abilityGroup.Name);
                    else
                    {
                        subAbilityGroupDefinition = GenerateAbilityGroupDefinition(subAbilityGroup, abilityGroups);
                        subAbilityGroupDefinitions.Add(subAbilityGroupDefinition);
                    }
                }

            }
            var abilityDefinitions = new List<IAbilityDefinition>();
            foreach (var abilityName in abilityGroup.Abilities)
            {
                var abilityDefinition = AbilityManager[abilityName];
                if (abilityDefinition == null)
                    Logger.LogError("Ability [{abilityName}] defined in group [{abilityGroupName}] doesn't exist", abilityName, abilityGroup.Name);
                else
                {
                    abilityDefinitions.Add(abilityDefinition);
                }
            }
            var helpAttribute = abilityGroup.GetType().GetCustomAttribute<HelpAttribute>();
            var oneLineHelpAttribute = abilityGroup.GetType().GetCustomAttribute<OneLineHelpAttribute>();
            var abilityGroupDefinition = new AbilityGroupDefinition(abilityGroup.Name, subAbilityGroupDefinitions, abilityDefinitions, helpAttribute, oneLineHelpAttribute);
            _abilityGroupByName.Add(abilityGroup.Name, abilityGroupDefinition);
            return abilityGroupDefinition;
        }
    }
}
