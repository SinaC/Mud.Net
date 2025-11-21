using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
[Help(@"skill in ball-and-chain type weapons")]
public class Flail : PassiveBase
{
    private const string PassiveName = "Flail";

    public Flail(ILogger<Flail> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
