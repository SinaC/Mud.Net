using Mud.Server.Common;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;

namespace Mud.POC.Abilities2
{
    public abstract class DefensiveSpellBase : SpellBase
    {
        protected ICharacter Victim { get; set; }

        protected DefensiveSpellBase(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            if (abilityActionInput.Parameters.Length < 1)
                Victim = Caster;
            else
            {
                Victim = FindHelpers.FindByName(Caster.Room.People, abilityActionInput.Parameters[0]);
                if (Victim == null)
                    return "They aren't here.";
            }
            // victim found
            return null;
        }
    }
}
