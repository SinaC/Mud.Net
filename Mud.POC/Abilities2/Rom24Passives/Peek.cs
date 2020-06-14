using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
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
