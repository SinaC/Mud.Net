namespace Mud.POC.DeferredAction;

public abstract class PlayerCommand
{
    public abstract void Execute(Mob player, World world);
}
