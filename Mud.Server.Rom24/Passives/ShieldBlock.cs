using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 6)]
    public class ShieldBlock : PassiveBase
    {
        public const string PassiveName = "Shield Block";

        public ShieldBlock(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
        {
            if (user.Position <= Positions.Sleeping)
                return false;

            if (user.GetEquipment<IItemShield>(EquipmentSlots.OffHand) == null)
                return false;

            int chance = 3 + learnPercentage / 5;
            chance += user.Level - victim.Level;

            return diceRoll < chance;
        }
    }
}
