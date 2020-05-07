using System.Collections.Generic;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Common;

namespace Mud.Server.Races
{
    public abstract class RaceBase : IRace
    {
        private readonly List<AbilityUsage> _abilities;

        private readonly List<EquipmentSlots> _basicSlots = new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Light,
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Amulet,
            Domain.EquipmentSlots.Shoulders,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Cloak,
            Domain.EquipmentSlots.Waist,
            Domain.EquipmentSlots.Wrists,
            Domain.EquipmentSlots.Arms,
            Domain.EquipmentSlots.Hands,
            Domain.EquipmentSlots.Ring,
            Domain.EquipmentSlots.Ring,
            Domain.EquipmentSlots.Legs,
            Domain.EquipmentSlots.Feet,
            Domain.EquipmentSlots.Trinket,
            Domain.EquipmentSlots.Trinket,
            Domain.EquipmentSlots.MainHand,
            Domain.EquipmentSlots.OffHand,
        };

        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        #region IRace

        public abstract string Name { get; }

        public string DisplayName => Name.UpperFirstLetter();

        public abstract string ShortName { get; }

        public IEnumerable<AbilityUsage> Abilities => _abilities;

        public virtual IEnumerable<EquipmentSlots> EquipmentSlots => _basicSlots;

        public abstract int GetAttributeModifier(CharacterAttributes attribute);

        #endregion

        protected RaceBase()
        {
            _abilities = new List<AbilityUsage>();
        }

        protected void AddAbility(int level, string abilityName, int rating)
        {
            AddAbility(level, abilityName, null, 0, CostAmountOperators.None, rating);
        }

        protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add unknown ability [{0}] to race [{1}]", abilityName, Name);
                return;
            }
            //
            AddAbility(level, ability, resourceKind, costAmount, costAmountOperator, rating);
        }

        protected void AddAbility(int level, IAbility ability, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
        {
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
