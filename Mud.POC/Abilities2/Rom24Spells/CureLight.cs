using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CureLight : HealSpellBase
    {
        public override int Id => 26;
        public override string Name => "Cure Light";

        public CureLight(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";

        protected override int HealValue(int level) => RandomManager.Dice(1, 8) + level / 3;
    }
}
