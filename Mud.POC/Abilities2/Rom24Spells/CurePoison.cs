using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cure Poison", AbilityEffects.Cure)]
    public class CurePoison : CureSpellBase
    {
        public CurePoison(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet, abilityManager)
        {
        }

        protected override string ToCureAbilityName => "Poison";
        protected override string SelfNotFoundMsg => "You aren't poisoned.";
        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be poisoned.";
    }
}
