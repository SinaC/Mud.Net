using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.POC.Abilities2
{
    public class AbilityManager : IAbilityManager
    {
        private readonly Dictionary<string, AbilityInfo> _abilities;

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
            // TODO: we have to register every GameAction in DependencyContainer
            _abilities = new Dictionary<string, AbilityInfo>();
            Type iAbility = typeof(IAbility);
            foreach (var abilityType in Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iAbility.IsAssignableFrom(x)))
            {
                AbilityInfo abilityInfo = new AbilityInfo(abilityType);
                _abilities.Add(abilityInfo.Name, abilityInfo);
            }
        }
    }
}
