using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Healing)]
    public class CureLight : HealSpellBase
    {
        public const string SpellName = "Cure Light";

        public CureLight(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";
        protected override int HealValue => RandomManager.Dice(1, 8) + Level / 3;
    }
}
