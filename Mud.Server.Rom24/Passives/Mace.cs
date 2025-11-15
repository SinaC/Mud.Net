using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
public class Mace : PassiveBase
{
    private const string PassiveName = "Mace";

    public Mace(IRandomManager randomManager)
        : base(randomManager)
    {
    }
}
