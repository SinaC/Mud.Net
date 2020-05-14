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

        public abstract IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

        public abstract BasicAttributes PrimeAttribute { get; }

        public IEnumerable<AbilityUsage> Abilities => _abilities;

        public abstract int MaxPracticePercentage { get; }

        public abstract (int thac0_00, int thac0_32) Thac0 { get; }

        public abstract int MinHitPointGainPerLevel { get; }

        public abstract int MaxHitPointGainPerLevel { get; }

        #endregion

        protected ClassBase()
        {
            _abilities = new List<AbilityUsage>();
        }

        protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add unknown ability [{0}] to class [{1}]", abilityName, Name);
                return;
            }
            //
            _abilities.Add(new AbilityUsage
            {
                Ability = ability,
                Level = level,
                ResourceKind = resourceKind,
                CostAmount = costAmount,
                CostAmountOperator = costAmountOperator,
                Rating = rating
            });
        }
    }
}
