using Mud.Common;
using Mud.Container;
using Mud.Logger;
using Mud.Server.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.POC.Abilities2
{
    public class AbilityManager : IAbilityManager
    {
        private readonly Dictionary<string, IAbilityInfo> _abilities;

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
            return Abilities.Where(x => x.Type == type).FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, pattern));
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

        public AbilityManager()
        {
            _abilities = new Dictionary<string, IAbilityInfo>();
            // Get abilities and register them in IOC
            Type iAbility = typeof(IAbility);
            foreach (var abilityType in Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iAbility.IsAssignableFrom(x)))
            {
                AbilityInfo abilityInfo = new AbilityInfo(abilityType);
                if (_abilities.ContainsKey(abilityInfo.Name))
                    Log.Default.WriteLine(LogLevels.Error, "Duplicate ability {0}", abilityInfo.Name);
                else
                {
                    _abilities.Add(abilityInfo.Name, abilityInfo);
                    DependencyContainer.Current.Register(abilityType);
                }
            }
        }
    }
}
