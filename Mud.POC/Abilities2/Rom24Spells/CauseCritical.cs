using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cause Critical", AbilityEffects.Damage)]
    public class CauseCritical : CauseSpellBase
    {
        public CauseCritical(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue => RandomManager.Dice(3, 8) + Level - 6;
    }
}
