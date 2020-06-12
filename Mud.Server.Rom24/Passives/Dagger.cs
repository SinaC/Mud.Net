using Mud.Server.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Dagger : PassiveBase
    {
        public const string PassiveName = "Dagger";

        public Dagger(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
