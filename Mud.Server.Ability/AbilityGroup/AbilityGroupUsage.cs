using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Ability.AbilityGroup
{
    public class AbilityGroupUsage : IAbilityGroupUsage
    {
        public string Name { get; }

        public int Cost { get; }
        public bool IsBasics { get; }

        public IAbilityGroupDefinition AbilityGroupDefinition { get; }

        public AbilityGroupUsage(string name, int cost, bool isBasics, IAbilityGroupDefinition abilityGroupDefinition)
        {
            Name = name;
            Cost = cost;
            IsBasics = isBasics;
            AbilityGroupDefinition = abilityGroupDefinition;
        }
    }
}
