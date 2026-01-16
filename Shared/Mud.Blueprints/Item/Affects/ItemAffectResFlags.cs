using Mud.Server.Flags.Interfaces;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectResFlags : ItemAffectBase // used for WhereToResist
{
    public IIRVFlags IRVFlags { get; set; } = default!;
}
