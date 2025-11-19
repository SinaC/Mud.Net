using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.AdditionalAbilities;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
public class ThirdWield : PassiveBase
{
    private const string PassiveName = "Third Wield";

    public ThirdWield(ILogger<ThirdWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
