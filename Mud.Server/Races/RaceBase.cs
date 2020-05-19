using System.Collections.Generic;
using Mud.Container;
using Mud.Domain;
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
            Domain.EquipmentSlots.MainHand,
            Domain.EquipmentSlots.OffHand,
            Domain.EquipmentSlots.Float,
        };

        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();
        protected IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();

        #region IRace

        public abstract string Name { get; }

        public string DisplayName => Name.UpperFirstLetter();

        public abstract string ShortName { get; }

        public IEnumerable<AbilityUsage> Abilities => _abilities;

        public virtual IEnumerable<EquipmentSlots> EquipmentSlots => _basicSlots;

        public abstract Sizes Size { get; }

        public abstract IRVFlags Immunities { get; }
        public abstract IRVFlags Resistances { get; }
        public abstract IRVFlags Vulnerabilities { get; }

        public abstract int GetStartAttribute(CharacterAttributes attribute);

        public abstract int GetMaxAttribute(CharacterAttributes attribute);

        public virtual int ClassExperiencePercentageMultiplier(IClass c) => 100;

        #endregion

        protected RaceBase()
        {
            _abilities = new List<AbilityUsage>();
        }

        protected void AddAbility(string abilityName)
        {
            AddAbility(1, abilityName, null, 0, CostAmountOperators.None, 1);
        }

        protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
            {
                Wiznet.Wiznet($"Trying to add unknown ability [{abilityName}] to race [{Name}]", WiznetFlags.Bugs, AdminLevels.Implementor);
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
