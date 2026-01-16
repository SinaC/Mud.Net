using Mud.Domain;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectSex : ItemAffectBase // used for WhereToAttributeOrResource
{
    public Sex Sex { get; set; }
}
