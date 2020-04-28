using Mud.Server.Helpers;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class AbilityManagerMock : IAbilityManager
    {
        private List<IAbility> _abilities = new List<IAbility>();

        public IAbility this[int id] => _abilities.SingleOrDefault(x => x.Id == id);

        public IAbility this[string name] => _abilities.SingleOrDefault(x => x.Name == name);

        public IAbility WeakenedSoulAbility => this["Weakened Soul"];

        public IAbility ParryAbility => this["Parry"];

        public IAbility DodgeAbility => this["Dodge"];

        public IAbility ShieldBlockAbility => this["Shield Block"];

        public IAbility DualWieldAbility => this["Dual Wield"];

        public IAbility ThirdWieldAbility => this["Third Wield"];

        public IAbility FourthWieldAbility => this["Fourth Wield"];

        public IEnumerable<IAbility> Abilities => _abilities;

        public bool Process(ICharacter source, params CommandParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public IAbility Search(CommandParameter parameter, bool includePassive = false)
        {
            return _abilities.Where(x =>
                     (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed
                     && (!includePassive || (x.Flags & AbilityFlags.Passive) != AbilityFlags.Passive)
                     && FindHelpers.StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public void AddAbility(IAbility ability)
        {
            _abilities.Add(ability);
        }

        public bool Process(ICharacter source, ICharacter target, IAbility ability, int level)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _abilities.Clear();
        }
    }
}
