using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 6)]
    public class ThirdAttack : PassiveBase
    {
        public const string PassiveName = "Third Attack";

        public ThirdAttack(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
        {
            if (user.CharacterFlags.HasFlag(CharacterFlags.Slow))
                return false;
            int chance = learnPercentage / 2;

            return diceRoll < chance;
        }

    }
}
