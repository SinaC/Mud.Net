using Mud.Server.Input;
using System;
using System.Collections.Generic;

namespace Mud.Server.Tests.Mocking
{
    internal class AbilityManagerMock : IAbilityManager
    {
        public IAbility this[int id] => throw new NotImplementedException();

        public IAbility this[string name] => throw new NotImplementedException();

        public IAbility WeakenedSoulAbility => throw new NotImplementedException();

        public IAbility ParryAbility => throw new NotImplementedException();

        public IAbility DodgerAbility => throw new NotImplementedException();

        public IAbility ShieldBlockAbility => throw new NotImplementedException();

        public IAbility DualWieldAbility => throw new NotImplementedException();

        public IAbility ThirdWieldAbility => throw new NotImplementedException();

        public IAbility FourthWieldAbility => throw new NotImplementedException();

        public IEnumerable<IAbility> Abilities => throw new NotImplementedException();

        public bool Process(ICharacter source, params CommandParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public bool Process(ICharacter source, ICharacter target, IAbility ability)
        {
            throw new NotImplementedException();
        }

        public IAbility Search(CommandParameter parameter, bool includePassive = false)
        {
            throw new NotImplementedException();
        }
    }
}
