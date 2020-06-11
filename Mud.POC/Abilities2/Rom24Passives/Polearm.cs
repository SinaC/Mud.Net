using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class Polearm : PassiveBase
    {
        public const string PassiveName = "Polearm";

        public Polearm(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
