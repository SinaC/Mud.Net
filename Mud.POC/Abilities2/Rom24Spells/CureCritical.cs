using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CureCritical : HealSpellBase
    {
        public override int Id => 24;
        public override string Name => "Cure Critical";

        public CureCritical(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";

        protected override int HealValue(int level) => RandomManager.Dice(3, 8) + level - 6;
    }
}
