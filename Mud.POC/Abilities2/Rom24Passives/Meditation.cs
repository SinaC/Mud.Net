using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 8)]
    public class Meditation : PassiveBase
    {
        public const string PassiveName = "Meditation";

        public Meditation(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
