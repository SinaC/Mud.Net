using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public class ItemFlagsAffect : FlagAffectBase<ItemFlags>, IItemAffect<IItem>
    {
        public void Apply(IItem item)
        {
            throw new System.NotImplementedException();
        }
    }
}
