namespace Mud.POC.Affects
{
    public class ItemFlagsAffect : FlagAffectBase<ItemFlags>, IItemAffect<IItem>
    {
        protected override string Target => "Item flags";

        public void Apply(IItem item)
        {
            item.ApplyAffect(this);
        }
    }
}
