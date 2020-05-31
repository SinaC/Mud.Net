using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CauseSerious : CauseSpellBase
    {
        public override int Id => 11;
        public override string Name => "Cause Serious";

        public CauseSerious(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int DamageValue(ICharacter caster, int level, ICharacter victim) => RandomManager.Dice(2, 8) + level / 2;
    }
}
