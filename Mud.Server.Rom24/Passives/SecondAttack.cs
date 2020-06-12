using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class SecondAttack : PassiveBase
    {
        public const string PassiveName = "Second Attack";

        public SecondAttack(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
        {
            int chance = learnPercentage / 2;
            if (user.CharacterFlags.HasFlag(CharacterFlags.Slow))
                chance /= 2;

            return diceRoll < chance;
        }
    }
}
