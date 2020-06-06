using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Identify", AbilityEffects.Detection)]
    public class Identify : ItemInventorySpellBase
    {
        public Identify(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            // TODO
            Caster.Send("Not Yet Implemented.");
        }
    }
}
