using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Affects.Item;

[Affect("ItemFlagsAffect", typeof(ItemFlagsAffectData))]
public class ItemFlagsAffect : FlagsAffectBase<IItemFlags>, IItemFlagsAffect
{
    protected override string Target => "Item flags";

    public void Initialize(ItemFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = new ItemFlags(data.Modifier);
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
            Modifier = Modifier.Serialize()
        };
    }
}
