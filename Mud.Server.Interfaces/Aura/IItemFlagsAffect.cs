using Mud.Domain;

namespace Mud.Server.Interfaces.Aura
{
    public interface IItemFlagsAffect : IFlagAffect<ItemFlags>, IItemAffect<IItem>
    {
    }
}
