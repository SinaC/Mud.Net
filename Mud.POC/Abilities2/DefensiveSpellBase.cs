using Mud.Server.Common;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class DefensiveSpellBase : SpellBase, ITargetedAction
    {
        protected ICharacter Victim { get; set; }

        protected DefensiveSpellBase(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Room.People.Where(caster.CanSee);

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
            {
                Victim = spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter;
                if (Victim == null)
                    Victim = Caster;
                return null;
            }

            if (spellActionInput.Parameters.Length < 1)
                Victim = Caster;
            else
            {
                Victim = FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee), spellActionInput.Parameters[0]);
                if (Victim == null)
                    return "They aren't here.";
            }
            // victim found
            return null;
        }
    }
}
