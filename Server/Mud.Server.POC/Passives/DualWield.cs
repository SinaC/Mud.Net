using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.POC.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 4)]
public class DualWield : AdditionalWieldPassiveBase
{
    private const string PassiveName = "Dual Wield";

    protected override string Name => PassiveName;

    public DualWield(ILogger<DualWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override int AdditionalHitIndex => 2;
    public override bool StopMultiHitIfFailed => false; // continue multi hit even if dual wield failed

    protected override int WieldCount => 2;

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        var chance = 2 * learnPercentage / 3 + 33;
        if (!user.CanSee(victim))
            chance = 2 * chance / 3;

        return diceRoll < chance;
    }
}
