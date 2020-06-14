using Mud.Domain;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Affect
{
    public interface IItemFlagsAffect : IFlagAffect<ItemFlags>, IItemAffect<IItem>
    {
    }
}
