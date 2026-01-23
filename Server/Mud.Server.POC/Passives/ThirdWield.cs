using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.POC.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
public class ThirdWield : AdditionalWieldPassiveBase
{
    private const string PassiveName = "Third Wield";

    protected override string Name => PassiveName;

    public ThirdWield(ILogger<ThirdWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override int AdditionalHitIndex => 3;
    public override bool StopMultiHitIfFailed => false; // continue multi hit even if dual wield failed

    protected override int WieldCount => 3;

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        var chance = learnPercentage / 3;
        if (!user.CanSee(victim))
            chance = 2 * chance / 3;

        return diceRoll < chance;
    }
}
