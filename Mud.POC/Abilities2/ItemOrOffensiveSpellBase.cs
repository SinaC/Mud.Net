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
        public ItemOrOffensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
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

        protected override void Invoke(ICharacter caster, int level, IEntity entity, string rawParameters, params CommandParameter[] parameters)
        {
            if (entity == null)
                return;
            if (entity is ICharacter victim)
                Action(caster, level, victim);
            else if (entity is IItem item)
                Action(caster, level, item);
            else
                Wiznet.Wiznet($"{GetType().Name}: invalid target type {entity.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            INonPlayableCharacter npcCaster = caster as INonPlayableCharacter;
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
            return AbilityTargetResults.Ok;
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, ICharacter victim);
        public abstract void Action(ICharacter caster, int level, IItem item);
    }
}
