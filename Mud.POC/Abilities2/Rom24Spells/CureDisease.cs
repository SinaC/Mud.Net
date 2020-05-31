using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CureDisease : CureSpellBase
    {
        public override int Id => 25;
        public override string Name => "Cure Disease";

        public CureDisease(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string ToCureAbilityName => "Plague";

        protected override string SelfNotFoundMsg => "You aren't ill.";

        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be diseased.";
    }
}
