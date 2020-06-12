using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Axe : PassiveBase
    {
        public const string PassiveName = "Axe";

        public Axe(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
