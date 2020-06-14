using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 6)]
    public class Dodge : PassiveBase
    {
        public const string PassiveName = "Dodge";

        public Dodge(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
        {
            if (user.Position <= Positions.Sleeping)
                return false;

            int chance = learnPercentage / 2;
            
            if (!user.CanSee(victim))
                chance /= 2;

            chance += user.Level - victim.Level;

            return diceRoll < chance;
        }
    }
}
