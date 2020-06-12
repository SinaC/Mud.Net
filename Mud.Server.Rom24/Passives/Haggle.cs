using Mud.Server.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 4)]
    public class Haggle : PassiveBase
    {
        public const string PassiveName = "Haggle";

        public Haggle(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
