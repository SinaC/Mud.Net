using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Sword : PassiveBase
    {
        public const string PassiveName = "Sword";

        public Sword(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
