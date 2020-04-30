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
        private readonly List<AbilityAndLevel> _abilities;

        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        #region IClass

        public abstract string Name { get; }

        public string DisplayName => Name.UpperFirstLetter();

        public abstract string ShortName { get; }

        public abstract IEnumerable<ResourceKinds> ResourceKinds { get; }

        public IEnumerable<AbilityAndLevel> Abilities => _abilities;

        public abstract IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

        public abstract int GetAttributeByLevel(CharacterAttributes attribute, int level);

        #endregion

        protected ClassBase()
        {
            _abilities = new List<AbilityAndLevel>();
        }

        public void AddAbility(int level, int abilityId)
        {
            IAbility ability = AbilityManager[abilityId];
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
            IAbility ability = AbilityManager[abilityName];
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
