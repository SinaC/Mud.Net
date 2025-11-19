using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.AdditionalAbilities;

[Passive(PassiveName, LearnDifficultyMultiplier = 4)]
public class DualWield : PassiveBase
{
    private const string PassiveName = "Dual Wield";

    public DualWield(ILogger<DualWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
