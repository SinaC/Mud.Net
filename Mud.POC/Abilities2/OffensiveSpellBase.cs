using Mud.Server.Common;
using Mud.Server.Input;
using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class OffensiveSpellBase : SpellBase
    {
        protected OffensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void PostInvoke(ICharacter caster, int level, IEntity target)
        {
            // multi hit if still in same room
            INonPlayableCharacter npcVictim = target as INonPlayableCharacter;
            if (target != caster
                && npcVictim?.Master != caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == target && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(caster);
                        break;
                    }
                }
            }
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            ICharacter victim;
            INonPlayableCharacter npcCaster = caster as INonPlayableCharacter;
            target = null;
            if (parameters.Length < 1)
            {
                victim = caster.Fighting;
                if (victim == null)
                {
                    caster.Send("Cast the spell on whom?");
                    return AbilityTargetResults.MissingParameter;
                }
            }
            else
                victim = FindHelpers.FindByName(caster.Room.People, parameters[0]);
            if (victim == null)
            {
                caster.Send("They aren't here.");
                return AbilityTargetResults.TargetNotFound;
            }
            if (caster is IPlayableCharacter)
            {
                if (caster != victim && victim.IsSafe(caster))
                {
                    caster.Send("Not on that Victim.");
                    return AbilityTargetResults.InvalidTarget;
                }
                // TODO: check_killer
            }
            if (npcCaster != null && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == victim)
            {
                caster.Send("You can't do that on your own follower.");
                return AbilityTargetResults.InvalidTarget;
            }
            // victim found
            target = victim;
            return AbilityTargetResults.Ok;
        }

        protected override void Invoke(ICharacter caster, int level, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            if (target == null)
                return;
            Action(caster, level, target as ICharacter);
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, ICharacter victim);
    }
}
