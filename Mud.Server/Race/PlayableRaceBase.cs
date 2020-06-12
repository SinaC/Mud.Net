using System.Collections.Generic;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Race
{
    public abstract class PlayableRaceBase : RaceBase, IPlayableRace
    {
        private readonly List<IAbilityUsage> _abilities;

        private readonly List<EquipmentSlots> _basicSlots = new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Light,
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Amulet, // 2 amulets
            Domain.EquipmentSlots.Amulet,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Cloak,
            Domain.EquipmentSlots.Waist,
            Domain.EquipmentSlots.Wrists, // 2 wrists
            Domain.EquipmentSlots.Wrists,
            Domain.EquipmentSlots.Arms,
            Domain.EquipmentSlots.Hands,
            Domain.EquipmentSlots.Ring, // 2 rings
            Domain.EquipmentSlots.Ring,
            Domain.EquipmentSlots.Legs,
            Domain.EquipmentSlots.Feet,
            Domain.EquipmentSlots.MainHand, // 2 hands
            Domain.EquipmentSlots.OffHand,
            Domain.EquipmentSlots.Float,
        };

        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();
        protected IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();

        #region IRace

        public abstract string ShortName { get; }

        public IEnumerable<IAbilityUsage> Abilities => _abilities;

        public override IEnumerable<EquipmentSlots> EquipmentSlots => _basicSlots;

        public abstract int GetStartAttribute(CharacterAttributes attribute);

        public abstract int GetMaxAttribute(CharacterAttributes attribute);

        public virtual int ClassExperiencePercentageMultiplier(IClass c) => 100;

        #endregion

        protected PlayableRaceBase()
        {
            _abilities = new List<IAbilityUsage>();
        }

        protected void AddAbility(string abilityName)
        {
            AddAbility(1, abilityName, null, 0, CostAmountOperators.None, 1);
        }

        protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
        {
            IAbilityInfo abilityInfo = AbilityManager[abilityName];
            if (abilityInfo == null)
            {
                Wiznet.Wiznet($"Trying to add unknown ability [{abilityName}] to race [{Name}]", WiznetFlags.Bugs, AdminLevels.Implementor);
                return;
            }
            //
            _abilities.Add(new AbilityUsage(abilityName, level, resourceKind, costAmount, costAmountOperator, rating, abilityInfo));
        }
    }
}
