using Mud.Server.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 8)]
    public class FastHealing : PassiveBase
    {
        public const string PassiveName = "Fast Healing";

        public FastHealing(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
