using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class SpellCauseCritical : SpellCauseBase
    {
        public override int Id => 9;

        public override string Name => "Cause Critical";

        public SpellCauseCritical(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue(int level) => RandomManager.Dice(3, 8) + level - 6;
    }
}
