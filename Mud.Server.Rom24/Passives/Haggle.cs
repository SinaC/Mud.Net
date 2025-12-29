using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 4)]
[Help(
@"Haggling is an indispensable skill to the trader.  It allows a character to
match wits with a merchant, seeking to get a better price for merchandise,
or to buy at the lowest possible cost.  Unfortunately, most merchants are 
already very skilled at haggling, so the untrainined adventurer had best 
guard his treasure closely.  Thieves are natural masters at haggling, 
although other classes may learn it as well.")]
public class Haggle : PassiveBase, IChangeCostPassive
{
    private const string PassiveName = "Haggle";

    protected override string Name => PassiveName;

    public Haggle(ILogger<Haggle> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public long HaggleBuyPrice(IPlayableCharacter buyer, INonPlayableCharacter keeper, long buyPrice)
    {
        var isTriggered = IsTriggered(buyer, keeper, false, out int diceRoll, out _);
        if (isTriggered)
        {
            buyer.Send("You haggle with the shopkeeper.");
            buyer.CheckAbilityImprove(PassiveName, true, 4); // improve only if success
            var newPrice = buyPrice - (buyPrice / 2 * diceRoll / 100);
            //cost += obj->cost / 2 * roll / 100;
            //cost = UMIN(cost, 95 * char_get_obj_cost(keeper, obj, TRUE) / 100);
            //cost = UMIN(cost, (keeper->silver + 100 * keeper->gold));
            return newPrice;
        }
        return buyPrice;
    }

    public long HaggleSellPrice(IPlayableCharacter seller, INonPlayableCharacter keeper, long sellPrice, long buyPrice)
    {
        var isTriggered = IsTriggered(seller, keeper, false, out int diceRoll, out _);
        if (isTriggered)
        {
            seller.Send("You haggle with the shopkeeper.");
            seller.CheckAbilityImprove(PassiveName, true, 4); // improve only if success
            var newPrice = sellPrice + (sellPrice / 2 * diceRoll / 100);
            newPrice = Math.Min(newPrice, buyPrice * 95 / 100);
            var keeperWealth = keeper.GoldCoins * 100 + keeper.SilverCoins;
            newPrice = Math.Min(newPrice, keeperWealth);
            return newPrice;
        }
        return sellPrice;
    }
}
