using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Affect.Item;

public interface IItemFlagsAffect : IFlagsAffect<IItemFlags>, IItemAffect<IItem>
{
}
