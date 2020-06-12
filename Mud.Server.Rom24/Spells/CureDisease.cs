using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Cure)]
    public class CureDisease : CureSpellBase
    {
        public const string SpellName = "Cure Disease";

        public CureDisease(IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
            : base(randomManager, abilityManager, dispelManager)
        {
        }

        protected override string ToCureAbilityName => Plague.SpellName;
        protected override string SelfNotFoundMsg => "You aren't ill.";
        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be diseased.";
    }
}
