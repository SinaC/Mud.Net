using System.Collections.Generic;
using Mud.Container;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Constants;
using Mud.Server.Helpers;

namespace Mud.Server.Classes
{
    public abstract class ClassBase : IClass
    {
        private readonly List<AbilityAndLevel> _abilities;

        #region IClass

        public abstract string Name { get; }

        public string DisplayName => StringHelpers.UpperFirstLetter(Name);

        public abstract string ShortName { get; }

        public abstract IEnumerable<ResourceKinds> ResourceKinds { get; }

        public IEnumerable<AbilityAndLevel> Abilities => _abilities;

        public abstract IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

        public abstract int GetPrimaryAttributeByLevel(PrimaryAttributeTypes primaryAttribute, int level);

        #endregion

        protected ClassBase()
        {
            _abilities = new List<AbilityAndLevel>();
        }

        public void AddAbility(int level, int abilityId)
        {
            IAbility ability = DependencyContainer.Instance.GetInstance<IAbilityManager>()[abilityId];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Trying to add unknown ability [id:{0}] to class [{1}]", abilityId, Name);
                return;
            }
            //
            AddAbility(level, ability);
        }

        public void AddAbility(int level, string abilityName)
        {
            IAbility ability = DependencyContainer.Instance.GetInstance<IAbilityManager>()[abilityName];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Trying to add unknown ability [{0}] to class [{1}]", abilityName, Name);
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
