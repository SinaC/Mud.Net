namespace Mud.POC.DeferredAction;

public class DeathAction : IGameAction
{
    private readonly Mob _mob;

    public DeathAction(Mob mob)
    {
        _mob = mob;
    }

    public void Execute(World world)
    {
        // Clear combat target
        _mob.CurrentTarget = null;

        // Clear any mobs who were targeting this mob
        world.ClearDeadMobTargets(_mob);

        // Enqueue loot drop
        world.Enqueue(new DropLootAction(_mob));

        // Enqueue XP grant
        world.Enqueue(new GrantExperienceAction(_mob));

        // Finally, schedule structural removal
        world.ScheduleRemoveMob(_mob);
    }
}
