using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 4)]
[Help(
@"Haggling is an indispensable skill to the trader.  It allows a character to
match wits with a merchant, seeking to get a better price for merchandise,
or to buy at the lowest possible cost.  Unfortunately, most merchants are 
already very skilled at haggling, so the untrainined adventurer had best 
guard his treasure closely.  Thieves are natural masters at haggling, 
although other classes may learn it as well.")]
public class Haggle : PassiveBase
{
    private const string PassiveName = "Haggle";

    public Haggle(ILogger<Haggle> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
