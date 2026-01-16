namespace Mud.Blueprints.Character;

public class CharacterShopBlueprintBase : CharacterBlueprintBase
{
    public int ProfitBuy { get; set; }
    public int ProfitSell { get; set; }
    public int OpenHour { get; set; }
    public int CloseHour { get; set; }
}
