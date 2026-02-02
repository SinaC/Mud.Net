using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.POC.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 8)]
public class FourthWield : AdditionalWieldPassiveBase
{
    private const string PassiveName = "Fourth Wield";

    protected override string Name => PassiveName;

    public FourthWield(ILogger<FourthWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override int AdditionalHitIndex => 4;
    public override bool StopMultiHitIfFailed => false; // continue multi hit even if dual wield failed

    protected override int WieldCount => 4;

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        var chance = learnPercentage / 5;
        if (!user.CanSee(victim))
            chance = 2 * chance / 3;

        return diceRoll < chance;
    }

}
