using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Healing)]
    public class CureCritical : HealSpellBase
    {
        public const string SpellName = "Cure Critical";

        public CureCritical(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";
        protected override int HealValue => RandomManager.Dice(3, 8) + Level - 6;
    }
}
