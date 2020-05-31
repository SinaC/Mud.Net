using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CauseCritical : CauseSpellBase
    {
        public override int Id => 9;
        public override string Name => "Cause Critical";

        public CauseCritical(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue(ICharacter caster, int level, ICharacter victim) => RandomManager.Dice(3, 8) + level - 6;
    }
}
