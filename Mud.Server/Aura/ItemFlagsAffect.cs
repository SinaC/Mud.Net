using Mud.Domain;

namespace Mud.Server.Aura
{
    public class ItemFlagsAffect : FlagAffectBase<ItemFlags>, IItemAffect<IItem>
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
