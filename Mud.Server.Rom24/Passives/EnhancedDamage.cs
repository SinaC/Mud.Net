using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
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
