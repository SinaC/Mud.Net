using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Spear : PassiveBase
    {
        public const string PassiveName = "Spear";

        public Spear(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
