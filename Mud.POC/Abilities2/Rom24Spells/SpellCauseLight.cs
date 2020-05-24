using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class SpellCauseLight : SpellCauseBase
    {
        public override int Id => 10;

        public override string Name => "Cause Light";

        public SpellCauseLight(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue(int level) => RandomManager.Dice(1, 8) + level / 3;
    }
}
