using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Affects.Item;

[Affect("ItemWeaponFlagsAffect", typeof(ItemWeaponFlagsAffectData))]
public class ItemWeaponFlagsAffect : FlagsAffectBase<IWeaponFlags>, IItemWeaponFlagsAffect
{
    protected override string Target => "Weapon flags";

    public void Initialize(ItemWeaponFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = new WeaponFlags(data.Modifier);
    }

    public void Apply(IItemWeapon item)
    {
        item.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new ItemWeaponFlagsAffectData
        {
            Operator = Operator,
            Modifier = Modifier.Serialize()
        };
    }
}
