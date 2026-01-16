using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_breath_gas")]
    public class BreathGas : BreathBase
    {
        public BreathGas(ILogger<BreathAcid> logger, IRandomManager randomManager)
            : base(logger, randomManager)
        {
        }

        protected override string GetSpellName() => "gas breath";
    }
}
