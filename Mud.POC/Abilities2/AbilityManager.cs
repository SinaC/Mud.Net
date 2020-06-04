using Mud.Container;
using Mud.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.POC.Abilities2
{
    public class AbilityManager : IAbilityManager
    {
        private readonly Dictionary<string, AbilityInfo> _abilities;

        public IEnumerable<AbilityInfo> Abilities => _abilities.Values;

        public AbilityInfo this[string abilityName]
        {
            get
            {
                if (!_abilities.TryGetValue(abilityName, out var abilityInfo))
                    return null;
                return abilityInfo;
            }
        }

        public AbilityManager()
        {
            _abilities = new Dictionary<string, AbilityInfo>();
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
