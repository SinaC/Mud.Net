using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Interfaces.AbilityGroup
{
    public interface IAbilityGroupInfo
    {
        string Name { get; }

        IEnumerable<IAbilityGroupInfo> AbilityGroupInfos { get; }
        IEnumerable<IAbilityInfo> AbilityInfos { get; }
    }
}
