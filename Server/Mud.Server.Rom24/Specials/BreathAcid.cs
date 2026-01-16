using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_breath_acid")]
    public class BreathAcid : BreathBase
    {
        public BreathAcid(ILogger<BreathAcid> logger, IRandomManager randomManager)
            : base(logger, randomManager)
        {
        }

        protected override string GetSpellName() => "acid breath";
    }
}
