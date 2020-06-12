﻿using Moq;
using Mud.Server.Ability;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
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

        TAbility IAbilityManager.CreateInstance<TAbility>(string abilityName) => throw new NotImplementedException();
    }
}
