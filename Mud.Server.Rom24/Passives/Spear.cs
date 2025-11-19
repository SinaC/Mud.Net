using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
public class Spear : PassiveBase
{
    private const string PassiveName = "Spear";

    public Spear(ILogger<Spear> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
