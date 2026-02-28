using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class LevelUpAction : IGameAction
{
    private readonly Mob _mob;

    public LevelUpAction(Mob mob)
    {
        _mob = mob;
    }

    public void Execute(World world)
    {
        _mob.Level++;
        _mob.MaxHitPoints += CombatFormulas.RollHitPoints(_mob.Level);
        _mob.HitPoints = _mob.MaxHitPoints; // restore HP on level-up
        _mob.MaxMana += 10;
        _mob.Mana = _mob.MaxMana; // restore mana

        // Notify scripts / triggers
        world.Enqueue(new ScriptAction(ctx =>
        {
            ctx.Notify($"{_mob} has reached level {_mob.Level}!");
        }));
    }
}
