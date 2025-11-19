using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
public class HandToHand : PassiveBase
{
    private const string PassiveName = "Hand to Hand";

    public HandToHand(ILogger<HandToHand> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
