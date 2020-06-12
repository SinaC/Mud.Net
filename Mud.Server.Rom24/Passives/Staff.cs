using Mud.Server.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Staff : PassiveBase
    {
        public const string PassiveName = "Staff(weapon)";

        public Staff(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
