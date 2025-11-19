using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.AdditionalAbilities;

[Passive(PassiveName, LearnDifficultyMultiplier = 8)]
public class FourthWield : PassiveBase
{
    private const string PassiveName = "Fourth Wield";

    public FourthWield(ILogger<FourthWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
