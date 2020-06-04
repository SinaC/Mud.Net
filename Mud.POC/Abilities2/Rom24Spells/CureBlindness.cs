using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cure Blindness", AbilityEffects.Cure)]
    public class CureBlindness : CureSpellBase
    {
        public CureBlindness(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet, abilityManager)
        {
        }

        protected override string ToCureAbilityName => "Blindness";
        protected override string SelfNotFoundMsg => "You aren't blind.";
        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be blinded.";
    }
}
