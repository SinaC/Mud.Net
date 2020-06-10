using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrDefensiveSpellBase : SpellBase
    {
        protected IEntity Target { get; set; }

        protected ItemOrDefensiveSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster)
            =>
            caster.Room.People.Where(caster.CanSee).OfType<IEntity>()
            .Concat(caster.Inventory.Where(caster.CanSee));

        protected override void Invoke()
        {
            if (Target is IItem item)
                Invoke(item);
            if (Target is ICharacter victim)
                Invoke(victim);
        }

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            Target = spellActionInput.Parameters.Length < 1
                        ? Caster
                        : FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee), spellActionInput.Parameters[0]);
            if (Target == null)
            {
                Target = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0]);
                if (Target == null)
                    return "You don't see that here.";
            }
            // victim or item (target) found
            return null;
        }

        protected abstract void Invoke(ICharacter victim);
        protected abstract void Invoke(IItem item);
    }
}
