using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Passive.Interfaces;

public interface IChangeCostPassive : IPassive
{
    long HaggleBuyPrice(IPlayableCharacter buyer, INonPlayableCharacter keeper, long buyPrice);
    long HaggleSellPrice(IPlayableCharacter seller, INonPlayableCharacter keeper, long sellPrice, long buyPrice);
}
