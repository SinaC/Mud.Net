using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Ability.AbilityGroup
{
    public class AbilityGroupInfo : IAbilityGroupInfo
    {
        public string Name { get; }

        public IEnumerable<IAbilityGroupInfo> AbilityGroupInfos { get; }
        public IEnumerable<IAbilityInfo> AbilityInfos { get; }

        public AbilityGroupInfo(string name, IEnumerable<IAbilityGroupInfo> abilityGroupInfos, IEnumerable<IAbilityInfo> abilityInfos)
        {
            Name = name;
            AbilityGroupInfos = abilityGroupInfos;
            AbilityInfos = abilityInfos;
        }
    }
}
