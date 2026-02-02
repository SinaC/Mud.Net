using Mud.Domain.SerializationData.Avatar;
using Mud.Server.AbilityGroup.Interfaces;

namespace Mud.Server.Ability.AbilityGroup;

public class AbilityGroupLearned : IAbilityGroupLearned
{
    public string Name { get; }

    public int Cost { get; }

    public IAbilityGroupDefinition AbilityGroupDefinition { get; }

    public AbilityGroupLearned(IAbilityGroupUsage abilityGroupUsage)
    {
        Name = abilityGroupUsage.Name;
        Cost = abilityGroupUsage.Cost;
        AbilityGroupDefinition = abilityGroupUsage.AbilityGroupDefinition;
    }

    public AbilityGroupLearned(LearnedAbilityGroupData learnedAbilityGroupData, IAbilityGroupDefinition abilityGroupDefinition)
    {
        Name = learnedAbilityGroupData.Name;
        Cost = learnedAbilityGroupData.Cost;
        AbilityGroupDefinition = abilityGroupDefinition;
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
