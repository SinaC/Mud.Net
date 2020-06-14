using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
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
