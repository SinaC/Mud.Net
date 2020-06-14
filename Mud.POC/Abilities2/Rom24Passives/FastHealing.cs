using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
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
