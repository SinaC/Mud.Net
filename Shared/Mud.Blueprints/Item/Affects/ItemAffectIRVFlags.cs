using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item.Affects;

public abstract class ItemAffectIRVFlags : ItemAffectBase
{
    public IIRVFlags IRVFlags { get; set; } = default!;
}
