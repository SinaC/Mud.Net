using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class HandToHand : PassiveBase
    {
        public const string PassiveName = "Hand to hand";

        public HandToHand(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
