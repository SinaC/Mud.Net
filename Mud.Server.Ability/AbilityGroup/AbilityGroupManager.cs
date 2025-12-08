using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Ability.AbilityGroup
{
    [Export(typeof(IAbilityGroupManager)), Shared]
    public class AbilityGroupManager : IAbilityGroupManager
    {
        private ILogger<AbilityGroupManager> Logger { get; }
        private IAbilityManager AbilityManager { get; }

        private readonly Dictionary<string, IAbilityGroupInfo> _abilityGroupByName;

        public AbilityGroupManager(ILogger<AbilityGroupManager> logger, IAbilityManager abilityManager, IEnumerable<IAbilityGroup> abilityGroups)
        {
            Logger = logger;
            AbilityManager = abilityManager;

            _abilityGroupByName = new Dictionary<string, IAbilityGroupInfo>(StringComparer.InvariantCultureIgnoreCase);
            GenerateAbilityGroupInfos(abilityGroups);
        }

        public IAbilityGroupInfo? this[string abilityGroupName]
        {
            get
            {
                if (!_abilityGroupByName.TryGetValue(abilityGroupName, out var abilityGroupInfo))
                    return null;
                return abilityGroupInfo;
            }
        }

        private void GenerateAbilityGroupInfos(IEnumerable<IAbilityGroup> abilityGroups)
        {
            foreach (var abilityGroup in abilityGroups)
            {
                if (!_abilityGroupByName.ContainsKey(abilityGroup.Name))
                {
                    GenerateAbilityGroupInfo(abilityGroup.Name, abilityGroup.AbilityGroups, abilityGroup.Abilities, abilityGroups);
                }
            }
        }

        private IAbilityGroupInfo GenerateAbilityGroupInfo(string groupName, IEnumerable<string> subGroupNames, IEnumerable<string> abilityNames, IEnumerable<IAbilityGroup> abilityGroups)
        {
            // TODO: handle cycle
            // TODO: what if a group is defined twice with a different ability composition ?
            if (_abilityGroupByName.TryGetValue(groupName, out var existingAbilityGroupInfo))
                return existingAbilityGroupInfo;
            var subAbilityGroupInfos = new List<IAbilityGroupInfo>();
            foreach (var subGroupName in subGroupNames)
            {
                if (_abilityGroupByName.TryGetValue(subGroupName, out var subAbilityGroupInfo))
                    subAbilityGroupInfos.Add(subAbilityGroupInfo);
                else
                {
                    var subAbilityGroup = abilityGroups.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, subGroupName));
                    if (subAbilityGroup == null)
                        Logger.LogError("Ability group [{subGroupName}] defined in ability group [{groupName}] doesn't exist", subGroupName, groupName);
                    else
                    {
                        subAbilityGroupInfo = GenerateAbilityGroupInfo(subGroupName, subAbilityGroup.AbilityGroups, subAbilityGroup.Abilities, abilityGroups);
                        subAbilityGroupInfos.Add(subAbilityGroupInfo);
                    }
                }

            }
            var abilityInfos = new List<IAbilityInfo>();
            foreach (var abilityName in abilityNames)
            {
                var abilityInfo = AbilityManager[abilityName];
                if (abilityInfo == null)
                    Logger.LogError("Ability [{abilityName}] defined in group [{abilityGroupName}] doesn't exist", abilityName, groupName);
                else
                {
                    abilityInfos.Add(abilityInfo);
                }
            }
            var abilityGroupInfo = new AbilityGroupInfo(groupName, subAbilityGroupInfos, abilityInfos);
            _abilityGroupByName.Add(groupName, abilityGroupInfo);
            return abilityGroupInfo;
        }
    }
}
