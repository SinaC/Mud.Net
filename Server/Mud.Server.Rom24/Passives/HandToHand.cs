using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
[Help(
@"Hand to hand combat is a rare skill in the lands of Midgaard.  Learning this
style of fighting gives the player a weapon even when disarmed -- bare hands.
Trained hand to hand experts are far more effective than many swordsmen.
Clerics and warriors are the best at this skill, although thieves and mages
may also learn it.")]
public class HandToHand : PassiveBase
{
    private const string PassiveName = "Hand to Hand";

    protected override string Name => PassiveName;

    public HandToHand(ILogger<HandToHand> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
