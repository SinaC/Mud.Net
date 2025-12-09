using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.GameAction;

public abstract class ItemGameActionBase<TItem, TItemGameActionInfo> : ActorGameActionBase<TItem, TItemGameActionInfo>
    where TItem : class, IItem
    where TItemGameActionInfo: class, IItemGameActionInfo
{
}
