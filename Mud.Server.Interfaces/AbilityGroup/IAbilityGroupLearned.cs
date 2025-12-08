using Mud.Domain.SerializationData;

namespace Mud.Server.Interfaces.AbilityGroup
{
    public interface IAbilityGroupLearned
    {
        string Name { get; }

        int Cost { get; }

        IAbilityGroupInfo AbilityGroupInfo { get; }

        LearnedAbilityGroupData MapLearnedAbilityGroupData();
    }
}
