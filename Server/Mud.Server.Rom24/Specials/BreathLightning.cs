using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_breath_lightning")]
public class BreathLightning : BreathBase
{
    public BreathLightning(ILogger<BreathAcid> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string GetSpellName() => "lightning breath";
}
