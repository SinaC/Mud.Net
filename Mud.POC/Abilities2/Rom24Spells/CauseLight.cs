using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cause Light", AbilityEffects.Damage)]
    public class CauseLight : CauseSpellBase
    {
        public CauseLight(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue => RandomManager.Dice(1, 8) + Level / 3;
    }
}
