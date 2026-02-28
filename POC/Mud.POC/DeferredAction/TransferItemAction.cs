namespace Mud.POC.DeferredAction;

public class TransferItemAction : IGameAction
{
    private readonly Item _item;
    private readonly IContainer _target;

    public TransferItemAction(Item item, IContainer target)
    {
        _item = item;
        _target = target;
    }

    public void Execute(World world)
    {
        if (_item.IsDeleted) return;

        world.ScheduleItemTransfer(_item, _target);
    }
}
