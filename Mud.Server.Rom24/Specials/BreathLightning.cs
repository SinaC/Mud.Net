using Microsoft.Extensions.Logging;
using Mud.Server.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_breath_lightning")]
    public class BreathLightning : BreathBase
    {
        public BreathLightning(ILogger<BreathAcid> logger, IRandomManager randomManager)
            : base(logger, randomManager)
        {
        }

        protected override string GetSpellName() => "lightning breath";
    }
}
