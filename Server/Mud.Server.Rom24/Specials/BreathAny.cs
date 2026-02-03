using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_breath_any")]
public class BreathAny : BreathBase
{
    public BreathAny(ILogger<BreathAcid> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string? GetSpellName()
    {
        var choice = RandomManager.Next(8);
        return choice switch
        {
            0 => "fire breath",
            1 or 2 => "lightning breath",
            3 => "gas breath",
            4 => "acid breath",
            5 or 6 or 7 => "frost breath",
            _ => null,
        };
    }
}
