using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Ability.AbilityGroup
{
    public class AbilityGroupLearned : IAbilityGroupLearned
    {
        public string Name { get; }

        public int Cost { get; }

        public IAbilityGroupInfo AbilityGroupInfo { get; }

        public AbilityGroupLearned(IAbilityGroupUsage abilityGroupUsage)
        {
            Name = abilityGroupUsage.Name;
            Cost = abilityGroupUsage.Cost;
            AbilityGroupInfo = abilityGroupUsage.AbilityGroupInfo;
        }

        public AbilityGroupLearned(LearnedAbilityGroupData learnedAbilityGroupData, IAbilityGroupInfo abilityGroupInfo)
        {
            Name = learnedAbilityGroupData.Name;
            Cost = learnedAbilityGroupData.Cost;
            AbilityGroupInfo = abilityGroupInfo;
        }

        public LearnedAbilityGroupData MapLearnedAbilityGroupData()
        {
            return new LearnedAbilityGroupData
            {
                Name = Name,
                Cost = Cost
            };
        }
    }
}
