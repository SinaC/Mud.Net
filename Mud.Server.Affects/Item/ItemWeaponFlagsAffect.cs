using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Affects.Item;

[Affect("ItemWeaponFlagsAffect", typeof(ItemWeaponFlagsAffectData))]
public class ItemWeaponFlagsAffect : FlagsAffectBase<IWeaponFlags, IWeaponFlagValues>, IItemWeaponFlagsAffect
{
    private IFlagFactory<IWeaponFlags, IWeaponFlagValues> FlagFactory { get; }

    public ItemWeaponFlagsAffect(IFlagFactory<IWeaponFlags, IWeaponFlagValues> flagFactory)
    {
        FlagFactory = flagFactory;
    }

    protected override string Target => "Weapon flags";

    public void Initialize(ItemWeaponFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = FlagFactory.CreateInstance(data.Modifier);
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
