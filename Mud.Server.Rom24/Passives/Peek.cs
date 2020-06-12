using Mud.Server.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 4)]
    public class Peek : PassiveBase
    {
        public const string PassiveName = "Peek";

        public Peek(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
