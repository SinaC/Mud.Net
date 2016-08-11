using System.Collections.Generic;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Helpers;

namespace Mud.Server.Races
{
    public abstract class RaceBase : IRace
    {
        private readonly List<AbilityAndLevel> _abilities;

        #region IRace

        public abstract string Name { get; }

        public string DisplayName => StringHelpers.UpperFirstLetter(Name);

        public abstract string ShortName { get; }

        public IEnumerable<AbilityAndLevel> Abilities => _abilities;

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
