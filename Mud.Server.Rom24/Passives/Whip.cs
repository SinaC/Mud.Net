using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
[Help(@"the use of whips, chains, and bullwhips")]
public class Whip : PassiveBase
{
    private const string PassiveName = "Whip";

    public Whip(ILogger<Whip> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
