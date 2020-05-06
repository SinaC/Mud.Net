using System.Collections.Generic;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Common;

namespace Mud.Server.Classes
{
    public abstract class ClassBase : IClass
    {
        private readonly List<AbilityUsage> _abilities;

        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        #region IClass

        public abstract string Name { get; }

        public string DisplayName => Name.UpperFirstLetter();

        public abstract string ShortName { get; }

        public abstract IEnumerable<ResourceKinds> ResourceKinds { get; }

        public IEnumerable<AbilityUsage> Abilities => _abilities;

        public abstract IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

        public abstract int GetAttributeByLevel(CharacterAttributes attribute, int level);

        #endregion

        protected ClassBase()
        {
            _abilities = new List<AbilityUsage>();
        }

        protected void AddAbility(int level, string abilityName, int improveDifficulityMultiplier)
        {
            AddAbility(level, abilityName, Domain.ResourceKinds.None, 0, CostAmountOperators.None, improveDifficulityMultiplier);
        }

        protected void AddAbility(int level, string abilityName, ResourceKinds resourceKind, int costAmount, CostAmountOperators costAmountOperator, int improveDifficulityMultiplier)
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add unknown ability [{0}] to class [{1}]", abilityName, Name);
                return;
            }
            //
            AddAbility(level, ability, resourceKind, costAmount, costAmountOperator, improveDifficulityMultiplier);
        }

        protected void AddAbility(int level, IAbility ability, ResourceKinds resourceKind, int costAmount, CostAmountOperators costAmountOperator, int improveDifficulityMultiplier)
        {
            _abilities.Add(new AbilityUsage
            {
                Ability = ability,
                Level = level,
                ResourceKind = resourceKind,
                CostAmount = costAmount,
                CostAmountOperator = costAmountOperator,
                DifficulityMultiplier = improveDifficulityMultiplier
            });
        }
    }
}
