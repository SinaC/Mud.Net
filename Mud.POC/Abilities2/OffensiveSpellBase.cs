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
        protected ICharacter Victim { get; private set; }

        protected OffensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        public override CastResults Cast(ICharacter caster, KnownAbility knownAbility, string rawParameters, params CommandParameter[] parameters)
        {
            CastResults result = base.Cast(caster, knownAbility, rawParameters, parameters);
            if (result != CastResults.Ok)
                return result;

            // multi hit if still in same room
            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            if (Victim != caster
                && npcVictim?.Master != caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == Victim && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(caster);
                        break;
                    }
                }
            }
            return CastResults.Ok;
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            ICharacter target;
            INonPlayableCharacter npcCaster = caster as INonPlayableCharacter;

            if (parameters.Length < 1)
            {
                target = caster.Fighting;
                if (target == null)
                {
                    caster.Send("Cast the spell on whom?");
                    return AbilityTargetResults.MissingParameter;
                }
            }
            else
                target = FindHelpers.FindByName(caster.Room.People, parameters[0]);
            if (target == null)
            {
                caster.Send("They aren't here.");
                return AbilityTargetResults.TargetNotFound;
            }
            if (caster is IPlayableCharacter)
            {
                if (caster != target && target.IsSafe(caster))
                {
                    caster.Send("Not on that Victim.");
                    return AbilityTargetResults.InvalidTarget;
                }
                // TODO: check_killer
            }
            if (npcCaster != null && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == target)
            {
                caster.Send("You can't do that on your own follower.");
                return AbilityTargetResults.InvalidTarget;
            }
            // victim found
            Victim = target;
            return AbilityTargetResults.Ok;
        }

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            if (Victim == null)
                return;
            Action(caster, level, Victim);
        }

        #endregion

        protected abstract void Action(ICharacter caster, int level, ICharacter victim);
    }
}
