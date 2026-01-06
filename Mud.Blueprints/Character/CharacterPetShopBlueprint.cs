namespace Mud.Blueprints.Character;

public class CharacterPetShopBlueprint : CharacterShopBlueprintBase
{
    public List<int> PetBlueprintIds { get; set; } = []; // assigned when parsing area file
    public List<CharacterNormalBlueprint> PetBlueprints { get; set; } = []; // assigned when each area is parsed
}
