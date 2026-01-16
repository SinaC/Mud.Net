using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item.Affects;

public class ItemAffectShieldFlags : ItemAffectBase // used for WhereToShields
{
    public IShieldFlags ShieldFlags { get; set; } = default!;
}