using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrOffensiveSpellBase : SpellBase
    {
        protected IEntity Target { get; private set; }

        public ItemOrOffensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
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
            INonPlayableCharacter npcVictim = Target as INonPlayableCharacter;
            if (Target != caster
                && npcVictim?.Master != caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == Target && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(caster);
                        break;
                    }
                }
            }
            return CastResults.Ok;
        }

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            if (Target == null)
                return;
            if (Target is ICharacter victim)
                Action(caster, level, victim);
            else if (Target is IItem item)
                Action(caster, level, item);
            else
                Wiznet.Wiznet($"{GetType().Name}: invalid target type {Target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            INonPlayableCharacter npcCaster = caster as INonPlayableCharacter;
            IEntity target;
            if (parameters.Length < 1)
            {
                target = caster.Fighting;
                if (target == null)
                {
                    caster.Send("Cast the spell on whom or what?");
                    return AbilityTargetResults.MissingParameter;
                }
            }
            else
                target = FindHelpers.FindByName(caster.Room.People, parameters[0]);
            if (target != null)
            {
                if (npcCaster != null && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == target)
                {
                    caster.Send("You can't do that on your own follower.");
                    return AbilityTargetResults.InvalidTarget;
                }
            }
            else // character not found, search item in room, in inventor, in equipment
            {
                target = FindHelpers.FindItemHere(caster, parameters[0]);
                if (target == null)
                {
                    caster.Send("You don't see that here.");
                    return AbilityTargetResults.TargetNotFound;
                }
            }
            // victim or item (target) found
            Target = target;
            return AbilityTargetResults.Ok;
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, ICharacter victim);
        public abstract void Action(ICharacter caster, int level, IItem item);
    }
}
