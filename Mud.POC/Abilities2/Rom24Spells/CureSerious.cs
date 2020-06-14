using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Healing)]
    public class CureSerious : HealSpellBase
    {
        public const string SpellName = "Cure Serious";

        public CureSerious(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";
        protected override int HealValue => RandomManager.Dice(2, 8) + Level / 2;
    }
}
