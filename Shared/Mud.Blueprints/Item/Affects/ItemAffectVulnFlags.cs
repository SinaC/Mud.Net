using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectVulnFlags : ItemAffectBase // used for WhereToVuln
{
    public IIRVFlags IRVFlags { get; set; } = default!;
}
