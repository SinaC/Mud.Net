using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Mace : PassiveBase
    {
        public const string PassiveName = "Mace";

        public Mace(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
