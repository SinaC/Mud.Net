using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.POC.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 8)]
public class FourthWield : PassiveBase
{
    private const string PassiveName = "Fourth Wield";

    protected override string Name => PassiveName;

    public FourthWield(ILogger<FourthWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    // TODO: check if a 3rd weapon is wielded
    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        return base.CheckSuccess(user, victim, learnPercentage, diceRoll);
    }
}
