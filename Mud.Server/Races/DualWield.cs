using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Races
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 4)]
    public class DualWield : PassiveBase
    {
        private const string PassiveName = "Dual Wield";

        public DualWield(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
