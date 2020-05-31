using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CauseLight : CauseSpellBase
    {
        public override int Id => 10;
        public override string Name => "Cause Light";

        public CauseLight(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue(ICharacter caster, int level, ICharacter victim) => RandomManager.Dice(1, 8) + level / 3;
    }
}
