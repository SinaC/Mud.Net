using Mud.Server.Ability.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Passive.Interfaces;

public interface IPassive : IAbility
{
    bool IsTriggered(ICharacter user, ICharacter victim, bool checkImprove, out int diceRoll, out int learnPercentage); // return true if passive was available and has been used, false otherwise. Dice roll is a value between 0 and 100 which has been used to check failure status
}
