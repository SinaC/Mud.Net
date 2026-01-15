using Mud.Domain;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectCharacterAttribute : ItemAffectBase // used for WhereToObject
{
    public CharacterAttributeAffectLocations Attribute { get; set; }
    public int Modifier { get; set; }
}
