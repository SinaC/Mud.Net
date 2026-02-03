using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.AbilityGroup.Interfaces;

public interface IAbilityGroupLearned
{
    string Name { get; }

    int Cost { get; }

    IAbilityGroupDefinition AbilityGroupDefinition { get; }

    LearnedAbilityGroupData MapLearnedAbilityGroupData();
}
