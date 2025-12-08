using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Ability.AbilityGroup
{
    public abstract class AbilityGroupBase : IAbilityGroup
    {
        private readonly List<string> _abilityGroups = [];
        private readonly List<string> _abilities = [];

        #region IAbilityGroup

        public abstract string Name { get; }

        public IEnumerable<string> AbilityGroups => _abilityGroups;
        public IEnumerable<string> Abilities => _abilities;

        #endregion

        public void AddAbilityGroup(string name)
        {
            _abilityGroups.Add(name);
        }

        public void AddAbility(string name)
        {
            _abilities.Add(name);
        }
    }
}
