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
        private readonly Dictionary<Type, IAbilityInfo[]> _abilitiesByExecutionType;

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
            //
            _abilitiesByExecutionType = new Dictionary<Type, IAbilityInfo[]>(); // will be filled at each call to AbilitiesByExecutionType
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

        public IEnumerable<IAbilityInfo> AbilitiesByExecutionType<TAbility>()
            where TAbility : class, IAbility
        {
            Type tAbilityType = typeof(TAbility);
            IAbilityInfo[] abilities;
            // Check in cache first
            if (_abilitiesByExecutionType.TryGetValue(tAbilityType, out abilities))
                return abilities;
            // Not found in cache, compute and put in cache
            abilities = Abilities.Where(x => tAbilityType.IsAssignableFrom(x.AbilityExecutionType)).ToArray();
            _abilitiesByExecutionType.Add(tAbilityType, abilities);
            return abilities;
        }

        public IAbilityInfo Search(string pattern, AbilityTypes type)
        {
            // TODO: use Trie ? or save in cache
            return Abilities.FirstOrDefault(x => x.Type == type && StringCompareHelpers.StringStartsWith(x.Name, pattern));
        }

        public IAbilityInfo Search(ICommandParameter parameter) => Abilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));

        public TAbility CreateInstance<TAbility>(string abilityName)
            where TAbility : class, IAbility
        {
            IAbilityInfo abilityInfo = this[abilityName];
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} doesn't exist.", abilityName);
                return default;
            }
            return CreateInstance<TAbility>(abilityInfo, abilityName);
        }

        public TAbility CreateInstance<TAbility>(IAbilityInfo abilityInfo)
            where TAbility : class, IAbility
            => CreateInstance<TAbility>(abilityInfo, abilityInfo.Name);

        #endregion

        private TAbility CreateInstance<TAbility>(IAbilityInfo abilityInfo, string abilityName)
             where TAbility : class, IAbility
        {
            if (DependencyContainer.Current.GetRegistration(abilityInfo.AbilityExecutionType, false) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} not found in DependencyContainer.", abilityName);
                return default;
            }
            TAbility instance = DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType) as TAbility;
            if (instance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} cannot be created or is not {1}.", abilityName, typeof(TAbility).Name);
                return default;
            }
            return instance;
        }
    }
}
