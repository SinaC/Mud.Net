using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Ability.AbilityGroup
{
    public class AbilityGroupUsage : IAbilityGroupUsage
    {
        public string Name { get; }

        public int Cost { get; }

        public IAbilityGroupInfo AbilityGroupInfo { get; }

        public AbilityGroupUsage(string name, int cost, IAbilityGroupInfo abilityGroupInfo)
        {
            Name = name;
            Cost = cost;
            AbilityGroupInfo = abilityGroupInfo;
        }
    }
}
