using System.Collections.Generic;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Constants;
using Mud.Server.Helpers;

namespace Mud.Server.Races
{
    public abstract class RaceBase : IRace
    {
        private readonly List<AbilityAndLevel> _abilities;
        private readonly List<EquipmentSlots> _basicSlots = new List<EquipmentSlots>
        {
                Constants.EquipmentSlots.Light,
                Constants.EquipmentSlots.Head,
                Constants.EquipmentSlots.Amulet,
                Constants.EquipmentSlots.Shoulders,
                Constants.EquipmentSlots.Chest,
                Constants.EquipmentSlots.Cloak,
                Constants.EquipmentSlots.Waist,
                Constants.EquipmentSlots.Wrists,
                Constants.EquipmentSlots.Arms,
                Constants.EquipmentSlots.Hands,
                Constants.EquipmentSlots.RingLeft,
                Constants.EquipmentSlots.RingRight,
                Constants.EquipmentSlots.Legs,
                Constants.EquipmentSlots.Feet,
                Constants.EquipmentSlots.Trinket1,
                Constants.EquipmentSlots.Trinket2,
                Constants.EquipmentSlots.Wield,
                Constants.EquipmentSlots.Wield2,
                Constants.EquipmentSlots.Shield,
                Constants.EquipmentSlots.Hold,
        };

        #region IRace

        public abstract string Name { get; }

        public string DisplayName => StringHelpers.UpperFirstLetter(Name);

        public abstract string ShortName { get; }

        public IEnumerable<AbilityAndLevel> Abilities => _abilities;

        public virtual IEnumerable<EquipmentSlots> EquipmentSlots => _basicSlots;

        #endregion

        protected RaceBase()
        {
            _abilities = new List<AbilityAndLevel>();
        }

        public void AddAbility(int level, int abilityId)
        {
            IAbility ability = Repository.AbilityManager[abilityId];
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
            IAbility ability = Repository.AbilityManager[abilityName];
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
