namespace Mud.Blueprints.Character;

public class CharacterShopBlueprint : CharacterShopBlueprintBase
{
    public List<Type> BuyBlueprintTypes { get; set; } = default!; // Type must implement ItemBlueprintBase
}
