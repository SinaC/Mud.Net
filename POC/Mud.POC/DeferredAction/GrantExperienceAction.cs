namespace Mud.POC.DeferredAction;

public class GrantExperienceAction : IGameAction
{
    private readonly Mob _victim;

    public GrantExperienceAction(Mob victim)
    {
        _victim = victim;
    }

    public void Execute(World world)
    {
        if (_victim.CurrentRoom == null) return;

        // Simple example: all mobs in room who attacked victim get XP
        var killers = world.GetMobsWhoAttacked(_victim);

        int baseXP = CombatFormulas.CalculateXP(_victim);

        foreach (var killer in killers)
        {
            // Schedule XP grant action
            world.Enqueue(new ApplyXPAction(killer, baseXP));
        }
    }
}
