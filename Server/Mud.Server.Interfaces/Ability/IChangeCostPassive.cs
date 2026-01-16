using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability
{
    public interface IChangeCostPassive : IPassive
    {
        long HaggleBuyPrice(IPlayableCharacter buyer, INonPlayableCharacter keeper, long buyPrice);
        long HaggleSellPrice(IPlayableCharacter seller, INonPlayableCharacter keeper, long sellPrice, long buyPrice);
    }
}
