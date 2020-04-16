using System.Collections.Generic;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Helpers;

namespace Mud.Server.Races
{
    public abstract class RaceBase : IRace
    {
        private readonly List<AbilityAndLevel> _abilities;

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
            Domain.EquipmentSlots.RingLeft,
            Domain.EquipmentSlots.RingRight,
            Domain.EquipmentSlots.Legs,
            Domain.EquipmentSlots.Feet,
            Domain.EquipmentSlots.Trinket1,
            Domain.EquipmentSlots.Trinket2,
            Domain.EquipmentSlots.Wield,
            Domain.EquipmentSlots.Wield2,
            Domain.EquipmentSlots.Shield,
            Domain.EquipmentSlots.Hold,
        };

        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        #region IRace

        public abstract string Name { get; }

        public string DisplayName => StringHelpers.UpperFirstLetter(Name);

        public abstract string ShortName { get; }

        public IEnumerable<AbilityAndLevel> Abilities => _abilities;

        public virtual IEnumerable<EquipmentSlots> EquipmentSlots => _basicSlots;

        public abstract int GetPrimaryAttributeModifier(PrimaryAttributeTypes primaryAttribute);

        #endregion

        protected RaceBase()
        {
            _abilities = new List<AbilityAndLevel>();
        }

        public void AddAbility(int level, int abilityId)
        {
            IAbility ability = AbilityManager[abilityId];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add unknown ability [id:{0}] to race [{1}]", abilityId, Name);
                return;
            }
            //
            AddAbility(level, ability);
        }

        public void AddAbility(int level, string abilityName)
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add unknown ability [{0}] to race [{1}]", abilityName, Name);
                return;
            }
            //
            AddAbility(level, ability);
        }

        protected void AddAbility(int level, IAbility ability)
        {
            _abilities.Add(new AbilityAndLevel(level, ability));
        }
    }
}
