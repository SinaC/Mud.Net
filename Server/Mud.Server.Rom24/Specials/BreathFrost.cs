using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_breath_frost")]
public class BreathFrost : BreathBase
{
    public BreathFrost(ILogger<BreathAcid> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string GetSpellName() => "frost breath";
}
