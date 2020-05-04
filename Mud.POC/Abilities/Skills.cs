using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Domain;

namespace Mud.POC.Abilities
{
    public static class Skills
    {
        [Skill(5000, "Berserk", AbilityTargets.CharacterOffensive)]
        public static bool Backstab(IAbility ability, ICharacter actor, ICharacter victim)
        {
            if (actor.Fighting != null)
            {
                actor.Send("You are facing the wrong end.");
                return false;
            }

            if (victim == actor)
            {
                actor.Send("How can you sneak up on yourself?");
                return false;
            }

            // TODO: is safe check
            // TODO: check kill stealing
            // TODO: check if wielding a weapon

            if (victim.HitPoints < victim.CurrentAttributes(CharacterAttributes.MaxHitPoints) / 3)
            {
                actor.Act(ActOptions.ToCharacter, "{0} is hurt and suspicious ... you can't sneak up.", victim);
                return false;
            }

            // TODO: check killer
            // TODO: GCD
            int learned = actor.KnownAbilities.FirstOrDefault(x => x.Ability == ability)?.Learned ?? 0;
            if ()
        }
    }
}
