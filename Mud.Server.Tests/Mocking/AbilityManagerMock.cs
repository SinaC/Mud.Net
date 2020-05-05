using Mud.Server.Common;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class AbilityManagerMock : IAbilityManager
    {
        private List<IAbility> _abilities = new List<IAbility>();

        public IAbility this[string name] => _abilities.SingleOrDefault(x => x.Name == name);

        public IEnumerable<IAbility> Abilities => _abilities;

        public IEnumerable<IAbility> Spells => _abilities.Where(x => x.Kind == Domain.AbilityKinds.Spell);

        public IEnumerable<IAbility> Skills => _abilities.Where(x => x.Kind == Domain.AbilityKinds.Skill);

        public IEnumerable<IAbility> Passives => _abilities.Where(x => x.Kind == Domain.AbilityKinds.Passive);

        public CastResults Cast(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public CastResults CastFromItem(IAbility ability, ICharacter caster, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public AbilityTargetResults GetAbilityTarget(IAbility ability, ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public AbilityTargetResults GetItemAbilityTarget(IAbility ability, ICharacter caster, ref IEntity target)
        {
            throw new NotImplementedException();
        }

        public UseResults Use(IAbility ability, ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        //
        public void AddAbility(IAbility ability)
        {
            _abilities.Add(ability);
        }
        public void Clear()
        {
            _abilities.Clear();
        }
    }
}
