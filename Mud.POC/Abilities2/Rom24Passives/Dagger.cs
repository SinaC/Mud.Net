using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Dagger : PassiveBase
    {
        public const string PassiveName = "Dagger";

        public Dagger(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
