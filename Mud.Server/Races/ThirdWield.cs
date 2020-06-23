using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Races
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 6)]
    public class ThirdWield : PassiveBase
    {
        private const string PassiveName = "Third Wield";

        public ThirdWield(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
