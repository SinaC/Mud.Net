using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
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
            if (user.Position <= Positions.Sleeping || user.Stunned > 0)
                return false;

            int chance = learnPercentage / 2;
            
            if (!user.CanSee(victim))
                chance /= 2;

            chance += user.Level - victim.Level;

            return diceRoll < chance;
        }
    }
}
