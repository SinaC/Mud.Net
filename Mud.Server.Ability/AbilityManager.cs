using Mud.Common;
using Mud.Container;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Ability
{
    public class AbilityManager : IAbilityManager
    {
        private readonly Dictionary<string, IAbilityInfo> _abilities; // TODO: trie to optimize Search ?

        public AbilityManager(IAssemblyHelper assemblyHelper)
        {
            _abilities = new Dictionary<string, IAbilityInfo>(StringComparer.InvariantCultureIgnoreCase);
            // Get abilities
            Type iAbility = typeof(IAbility);
            foreach (var abilityType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iAbility.IsAssignableFrom(t))))
            {
                AbilityInfo abilityInfo = new AbilityInfo(abilityType);
                if (_abilities.ContainsKey(abilityInfo.Name))
                    Log.Default.WriteLine(LogLevels.Error, "Duplicate ability {0}", abilityInfo.Name);
                else
                    _abilities.Add(abilityInfo.Name, abilityInfo);
            }
        }

        #region IAbilityManager

        public IEnumerable<IAbilityInfo> Abilities => _abilities.Values;

        public IAbilityInfo this[string abilityName]
        {
            get
            {
                if (!_abilities.TryGetValue(abilityName, out var abilityInfo))
                    return null;
                return abilityInfo;
            }
        }

        public IAbilityInfo Search(string pattern, AbilityTypes type)
        {
            // TODO: use Trie ?
            return Abilities.FirstOrDefault(x => x.Type == type && StringCompareHelpers.StringStartsWith(x.Name, pattern));
        }

        public IAbilityInfo Search(ICommandParameter parameter)
        {
            return Abilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));
        }

        public TAbility CreateInstance<TAbility>(string abilityName)
            where TAbility : class, IAbility
        {
            IAbilityInfo abilityInfo = this[abilityName];
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} doesn't exist.", abilityName);
                return default;
            }
            if (DependencyContainer.Current.GetRegistration(abilityInfo.AbilityExecutionType, false) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} not found in DependencyContainer.", abilityName);
                return default;
            }
            TAbility instance = DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType) as TAbility;
            if (instance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} cannot be instantiated or is not {1}.", abilityName, typeof(TAbility).Name);
                return default;
            }
            return instance;
        }

        #endregion
    }
}
