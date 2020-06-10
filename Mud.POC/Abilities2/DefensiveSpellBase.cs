using Mud.Server.Common;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class DefensiveSpellBase : SpellBase
    {
        protected ICharacter Victim { get; set; }

        protected DefensiveSpellBase(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster) => caster.Room.People.Where(caster.CanSee);

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
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
