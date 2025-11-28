using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Affects.Item;

[Affect("ItemFlagsAffect", typeof(ItemFlagsAffectData))]
public class ItemFlagsAffect : FlagsAffectBase<IItemFlags, IItemFlagValues>, IItemFlagsAffect
{
    protected override string Target => "Item flags";

    public ItemFlagsAffect()
    {
    }

    public ItemFlagsAffect(ItemFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = data.Modifier;
    }

    public void Apply(IItem item)
    {
        item.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new ItemFlagsAffectData
        {
            Operator = Operator,
            Modifier = Modifier
        };
    }
}
