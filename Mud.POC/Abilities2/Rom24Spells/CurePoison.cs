using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CurePoison : CureSpellBase
    {
        public override int Id => 27;
        public override string Name => "Cure Poison";

        public CurePoison(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string ToCureAbilityName => "Poison";

        protected override string SelfNotFoundMsg => "You aren't poisoned.";

        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be poisoned.";
    }
}
