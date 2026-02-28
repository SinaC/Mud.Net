namespace Mud.POC.DeferredAction;

public class DropLootAction : IGameAction
{
    private readonly Mob _mob;

    public DropLootAction(Mob mob)
    {
        _mob = mob;
    }

    public void Execute(World world)
    {
        if (_mob.Items.Count == 0) return;

        foreach (var item in _mob.Items.ToList()) // snapshot of items
        {
            // Move item from mob to room safely
            world.Enqueue(new TransferItemAction(item, _mob.CurrentRoom));
        }

        _mob.Items.Clear(); // logical state, lists will be updated in TransferItemAction
    }
}
