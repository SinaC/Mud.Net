using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
public class Axe : PassiveBase
{
    private const string PassiveName = "Axe";

    public Axe(ILogger<Axe> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
