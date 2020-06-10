using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrOffensiveSpellBase : SpellBase
    {
        protected IEntity Target { get; set; }

        protected ItemOrOffensiveSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override void Execute()
        {
            base.Execute();

            // multi hit if still in same room
            INonPlayableCharacter npcVictim = Target as INonPlayableCharacter;
            if (Target != Caster
                && npcVictim?.Master != Caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == Target && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(Caster);
                        break;
                    }
                }
            }
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster)
            =>
            caster.Room.People.Where(x => caster.CanSee(x) && IsTargetValid(caster, x)).OfType<IEntity>()
            .Concat(caster.Room.Content.Where(caster.CanSee))
            .Concat(caster.Inventory.Where(caster.CanSee))
            .Concat(caster.Equipments.Where(x => x.Item != null && caster.CanSee(x.Item)).Select(x => x.Item));

        protected override void Invoke()
        {
            if (Target is IItem item)
                Invoke(item);
            if (Target is ICharacter victim)
                Invoke(victim);
        }

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            INonPlayableCharacter npcCaster = Caster as INonPlayableCharacter;
            if (spellActionInput.Parameters.Length < 1)
            {
                Target = Caster.Fighting;
                if (Target == null)
                    return IsCastFromItem
                        ? "Use it on whom or what?"
                        : "Cast the spell on whom or what?";
            }
            else
                Target = FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee), spellActionInput.Parameters[0]);
            if (Target != null)
            {
                if (Caster is IPlayableCharacter)
                {
                    if (Caster != Target && ((ICharacter) Target).IsSafe(Caster))
                        return "Not on that victim.";
                    // TODO: check_killer
                }
                if (npcCaster != null && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == Target)
                    return "You can't do that on your own follower.";
            }
            else // character not found, search item in room, in inventor, in equipment
            {
                Target = FindHelpers.FindItemHere(Caster, spellActionInput.Parameters[0]);
                if (Target == null)
                    return "You don't see that here.";
            }
            // victim or item (target) found
            return null;
        }

        protected abstract void Invoke(ICharacter victim);
        protected abstract void Invoke(IItem item);

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
