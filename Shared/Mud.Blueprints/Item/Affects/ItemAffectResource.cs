using Mud.Domain;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectResource : ItemAffectBase // used for WhereToAttributeOrResource
{
    public ResourceKinds Location { get; set; }
    public int Modifier { get; set; }
}
