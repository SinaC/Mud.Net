using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.POC.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
public class ThirdWield : PassiveBase
{
    private const string PassiveName = "Third Wield";

    protected override string Name => PassiveName;

    public ThirdWield(ILogger<ThirdWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    // TODO: check if a 3rd weapon is wielded
    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        return base.CheckSuccess(user, victim, learnPercentage, diceRoll);
    }
}
