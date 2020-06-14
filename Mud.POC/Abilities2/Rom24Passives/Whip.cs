using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Whip : PassiveBase
    {
        public const string PassiveName = "Whip";

        public Whip(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
