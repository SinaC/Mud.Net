using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 6)]
    public class EnhancedDamage : PassiveBase
    {
        public const string PassiveName = "Enhanced Damage";

        public EnhancedDamage(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
