using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CureBlindness : CureSpellBase
    {
        public override int Id => 23;
        public override string Name => "Cure Blindness";

        public CureBlindness(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string ToCureAbilityName => "Blindness";
        protected override string SelfNotFoundMsg => "You aren't blind.";
        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be blinded.";
    }
}
