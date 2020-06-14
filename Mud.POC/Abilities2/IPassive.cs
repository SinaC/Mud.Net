using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2
{
    public interface IPassive : IAbility
    {
        bool Use(ICharacter user, ICharacter victim, out int diceRoll); // return true if passive was available and has been used, false otherwise. Dice roll is a value between 0 and 100 which has been used to check failure status
    }
}
