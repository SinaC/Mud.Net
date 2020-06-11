using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 4)]
    public class Haggle : PassiveBase
    {
        public const string PassiveName = "Haggle";

        public Haggle(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
