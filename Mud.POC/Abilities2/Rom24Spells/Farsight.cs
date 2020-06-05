using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Farsight", AbilityEffects.None)]
    public class Farsight : NoTargetSpellBase
    {
        public Farsight(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            Caster.Send("Not Yet Implemented.");
        }
    }
}
