using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CureSerious : HealSpellBase
    {
        public override int Id => 28;
        public override string Name => "Cure Serious";

        public CureSerious(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string HealVictimPhrase => "You feel better!";

        protected override int HealValue(int level) => RandomManager.Dice(2, 8) + level / 2;
    }
}
