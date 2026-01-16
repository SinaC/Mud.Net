using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectImmFlags : ItemAffectBase // used for WhereToImmune
{
    public IIRVFlags IRVFlags { get; set; } = default!;
}
