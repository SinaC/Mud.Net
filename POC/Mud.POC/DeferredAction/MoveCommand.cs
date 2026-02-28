namespace Mud.POC.DeferredAction;

public class MoveCommand : PlayerCommand
{
    private readonly Room _destination;

    public MoveCommand(Room destination)
    {
        _destination = destination;
    }

    public override void Execute(Mob player, World world)
    {
        if (player.IsDead) return;

        world.Enqueue(new EnterRoomAction(player, _destination));
    }
}
//Player movement triggers room enter actions.
//Any room triggers, mobs, or scripts will enqueue their actions safely.
