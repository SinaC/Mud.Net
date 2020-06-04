using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cure Serious", AbilityEffects.Healing)]
    public class CureSerious : HealSpellBase
    {
        public CureSerious(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";
        protected override int HealValue => RandomManager.Dice(2, 8) + Level / 2;
    }
}
