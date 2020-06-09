using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Cure)]
    public class CureBlindness : CureSpellBase
    {
        public const string SpellName = "Cure Blindness";

        public CureBlindness(IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
            : base(randomManager, abilityManager, dispelManager)
        {
        }

        protected override string ToCureAbilityName => Blindness.SpellName;
        protected override string SelfNotFoundMsg => "You aren't blind.";
        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be blinded.";
    }
}
