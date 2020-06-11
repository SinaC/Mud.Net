using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Affect
{
    public class ItemFlagsAffect : FlagAffectBase<ItemFlags>, IItemFlagsAffect
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
}
