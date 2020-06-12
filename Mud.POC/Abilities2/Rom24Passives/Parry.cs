using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 6)]
    public class Parry : PassiveBase
    {
        public const string PassiveName = "Parry";

        public Parry(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
        {
            if (user.Position <= Positions.Sleeping)
                return false;

            int chance = learnPercentage / 2;

            if (user.GetEquipment(EquipmentSlots.MainHand) == null)
            {
                if (user is IPlayableCharacter) // player must have a weapon to parry
                    return false;
                chance /= 2;
            }

            if (!user.CanSee(victim))
                chance /= 2;

            chance += user.Level - victim.Level;

            return diceRoll < chance;
        }
    }
}
