using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrDefensiveSpellBase : SpellBase
    {
        protected IEntity Target { get; set; }

        protected ItemOrDefensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            if (Target is IItem item)
                Invoke(item);
            if (Target is ICharacter victim)
                Invoke(victim);
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            Target = abilityActionInput.Parameters.Length < 1
                        ? Caster
                        : FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee), abilityActionInput.Parameters[0]);
            if (Target == null)
            {
                Target = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), abilityActionInput.Parameters[0]);
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
