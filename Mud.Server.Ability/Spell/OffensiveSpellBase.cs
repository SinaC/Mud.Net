using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Random;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Ability.Spell
{
    public abstract class OffensiveSpellBase : SpellBase, ITargetedAction
    {
        protected ICharacter Victim { get; set; }

        protected OffensiveSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override void Execute()
        {
            base.Execute();

            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            if (Victim != Caster
                && npcVictim?.Master != Caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == Victim && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(Caster);
                        break;
                    }
                }
            }
        }

        public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Room.People.Where(x => x != caster && caster.CanSee(x) && IsTargetValid(caster, x));

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
            {
                Victim = spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter ?? Caster.Fighting;
                if (Victim == null)
                    return "You can't do that.";
                return null;
            }

            if (spellActionInput.Parameters.Length < 1)
            {
                Victim = Caster.Fighting;
                if (Victim == null)
                    return IsCastFromItem
                        ? "Use it on whom?"
                        : "Cast the spell on whom?";
            }
            else
                Victim = FindHelpers.FindByName(Caster.Room.People, spellActionInput.Parameters[0]);
            if (Victim == null)
                return "They aren't here.";
            if (Caster is IPlayableCharacter)
            {
                if (Caster != Victim && Victim.IsSafe(Caster))
                    return "Not on that victim.";
                // TODO: check_killer
            }
            if (Caster is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == Victim)
                return "You can't do that on your own follower.";
            // victim found
            return null;
        }

        private bool IsTargetValid(ICharacter caster, ICharacter victim)
        {
            if (caster is IPlayableCharacter)
            {
                if (caster != victim && victim.IsSafe(caster))
                    return false;
            }
            if (caster is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == victim)
                return false;
            return true;
        }
    }
}
