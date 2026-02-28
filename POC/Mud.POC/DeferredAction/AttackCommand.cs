namespace Mud.POC.DeferredAction;

public class AttackCommand : PlayerCommand
{
    private readonly Mob _target;

    public AttackCommand(Mob target)
    {
        _target = target;
    }

    public override void Execute(Mob player, World world)
    {
        if (player.IsDead || _target.IsDead)
            return;

        if (player.IsCharmed && player.Master == _target)
            return;

        player.CurrentTarget = _target;

        // Enqueue attack for this tick
        world.Enqueue(new MultiHitAction(player));
    }
}

//Player can type attack goblin.
//Command finds the target, sets CurrentTarget, and enqueues attack actions.
//Cascading effects (counter-attacks, death, loot, XP) happen automatically.
