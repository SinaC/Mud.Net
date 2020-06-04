using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cure Critical", AbilityEffects.Healing)]
    public class CureCritical : HealSpellBase
    {
        public CureCritical(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";
        protected override int HealValue => RandomManager.Dice(3, 8) + Level - 6;
    }
}
