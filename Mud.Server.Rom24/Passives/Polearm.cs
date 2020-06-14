using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;
namespace Mud.Server.Rom24.Passives
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
