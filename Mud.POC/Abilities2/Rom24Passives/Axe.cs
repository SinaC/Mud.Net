using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Axe : PassiveBase
    {
        public const string PassiveName = "Axe";

        public Axe(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
