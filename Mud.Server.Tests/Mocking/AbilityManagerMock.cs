using Moq;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class AbilityManagerMock : IAbilityManager
    {
        private List<IAbilityInfo> _abilityInfos = new List<IAbilityInfo>();

        public IAbilityInfo this[string abilityName] 
        {
            get
            {
                IAbilityInfo info = _abilityInfos.FirstOrDefault(x => x.Name == abilityName);
                if (info != null)
                    return info;
                Mock<IAbilityInfo> infoMock = new Mock<IAbilityInfo>();
                _abilityInfos.Add(infoMock.Object);
                return infoMock.Object;
            }
        }

        public IEnumerable<IAbilityInfo> Abilities => _abilityInfos;

        public IAbilityInfo Search(string pattern, AbilityTypes type) => Abilities.FirstOrDefault(x => x.Type == type && x.Name.StartsWith(pattern, StringComparison.InvariantCultureIgnoreCase));

        public IAbilityInfo Search(ICommandParameter parameter) => Abilities.FirstOrDefault(x => x.Name.StartsWith(parameter.Value, StringComparison.InvariantCultureIgnoreCase));
        public IEnumerable<IAbilityInfo> SearchAbilities<TAbility>()
            where TAbility: class, IAbility
        {
            throw new NotImplementedException();
        }

        IEnumerable<IAbilityInfo> IAbilityManager.SearchAbilitiesByExecutionType<TAbility>() => Abilities.Where(x => x.AbilityExecutionType is TAbility);

        TAbility IAbilityManager.CreateInstance<TAbility>(string abilityName) => throw new NotImplementedException();

        TAbility IAbilityManager.CreateInstance<TAbility>(IAbilityInfo abilityInfo)
        {
            throw new NotImplementedException();
        }
    }
}
