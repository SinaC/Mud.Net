using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 4)]
[Help(
@"The peek skill is useful for seeing what a player or monster is carrying,
the better to use the steal command with.  More intelligent characters are
harder to peek at.  All characters may learn peek, but thieves are the most
common practicioners.")]
[OneLineHelp("used to look into a person's belongings")]
public class Peek : PassiveBase
{
    private const string PassiveName = "Peek";

    public Peek(ILogger<Peek> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
