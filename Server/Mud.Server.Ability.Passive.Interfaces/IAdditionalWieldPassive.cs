using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Ability.Passive.Interfaces;

public interface IAdditionalWieldPassive : IPassive
{
    int AdditionalHitIndex { get; }
    bool StopMultiHitIfFailed { get; }

    bool IsTriggered(ICharacter user, ICharacter victim, bool checkImprove, out int diceRoll, out int learnPercentage, out IItemWeapon? weapon);
}
