using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Flail : PassiveBase
    {
        public const string PassiveName = "Flail";

        public Flail(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
