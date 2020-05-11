using Mud.Server.Abilities;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class AbilityManagerMock : IAbilityManager
    {
        private readonly List<IAbility> _abilities = new List<IAbility>();

        public IAbility this[string name] 
        {
            get
            {
                IAbility ability = _abilities.SingleOrDefault(x => x.Name == name);
                if (ability != null)
                    return ability;
                int id = _abilities.Count == 0 ? 1 : _abilities.Max(x => x.Id) + 1;
                ability = new Ability(Domain.AbilityKinds.Passive, id, name, Domain.AbilityTargets.Custom, 12, Domain.AbilityFlags.None, string.Empty, string.Empty, string.Empty, 1);
                _abilities.Add(ability);
                return ability;
            }
        }

        public IAbility this[int id]
        {
            get
            {
                IAbility ability = _abilities.SingleOrDefault(x => x.Id == id);
                if (ability != null)
                    return ability;
                ability = new Ability(Domain.AbilityKinds.Passive, id, "ability"+id.ToString(), Domain.AbilityTargets.Custom, 12, Domain.AbilityFlags.None, string.Empty, string.Empty, string.Empty, 1);
                _abilities.Add(ability);
                return ability;
            }
        }

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

        public KnownAbility Search(IEnumerable<KnownAbility> knownAbilities, int level, Func<IAbility, bool> abilityFilterFunc, CommandParameter parameter) => knownAbilities.FirstOrDefault(x => abilityFilterFunc(x.Ability) && x.Ability.Name == parameter.Value);

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
