using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class HealSpell : Skill
{
    private readonly int _healAmount;

    public HealSpell() : base("Heal", 5)
    {
        _healAmount = 30;
    }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (caster.IsDead || target.IsDead) return;
        if (caster.Mana < ManaCost) return;

        caster.Mana -= ManaCost;

        world.Enqueue(new ScriptAction(ctx =>
        {
            target.HitPoints = Math.Min(target.HitPoints + _healAmount, target.MaxHitPoints);
            ctx.Notify($"{caster} heals {target} for {_healAmount} HP!");
        }));
    }
}