using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cure Disease", AbilityEffects.Cure)]
    public class CureDisease : CureSpellBase
    {
        public CureDisease(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet, abilityManager)
        {
        }

        protected override string ToCureAbilityName => "Plague";
        protected override string SelfNotFoundMsg => "You aren't ill.";
        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be diseased.";
    }
}
