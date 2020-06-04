using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cause Serious", AbilityEffects.Damage)]
    public class CauseSerious : CauseSpellBase
    {
        public CauseSerious(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue => RandomManager.Dice(2, 8) + Level / 2;
    }
}
