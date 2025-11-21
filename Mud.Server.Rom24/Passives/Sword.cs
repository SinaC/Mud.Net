using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
[Help(@"the warrior's standby, from rapier to claymore")]
public class Sword : PassiveBase
{
    private const string PassiveName = "Sword";

    public Sword(ILogger<Sword> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
