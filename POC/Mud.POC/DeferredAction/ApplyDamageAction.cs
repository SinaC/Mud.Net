using Mud.POC.DeferredAction.Combat;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

//Purpose: Full rule-driven damage application.
//Behavior:
//  Applies all combat modifiers:
//  Armor class / AC
//  Hitroll / Damroll bonuses
//  Sanctuary, protection, shield effects
//  Status effects(Blind, Poison, Faerie Fire, etc.)
//  Saving throws(Fortitude, Reflex, Will)
//  Triggers break-hide-on-attack logic.
//  Calls DamageAction internally to actually subtract HP after all adjustments.
//Use case:
//  Any attack in the modular combat pipeline: multi-hit, skills, spells, or backstab.
//  Ensures all ROM-style rules, resistances, and effects are applied consistently.
public class ApplyDamageAction : IGameAction
{
    private readonly Mob _attacker;
    private readonly Mob _target;
    private readonly int _baseDamage;

    public ApplyDamageAction(Mob attacker, Mob target, int baseDamage)
    {
        _attacker = attacker;
        _target = target;
        _baseDamage = baseDamage;
    }

    public void Execute(World world)
    {
        if (_target.IsDead) return;

        // 1️) Build the damage context
        var context = new CombatContext(_attacker, _target)
        {
            BaseDamage = _baseDamage
        };

        // 2️) Apply all StatusEffect / Rule Engine modifiers
        world.StatusEffectEngine.ApplyCombatModifiers(context);

        // 3️) Final damage cannot be negative
        context.FinalDamage = Math.Max(0, context.FinalDamage);

        // 4️) Enqueue raw DamageAction for HP subtraction and breaking sneak/hide
        world.Enqueue(new DamageAction(_attacker, _target, context.FinalDamage));
    }
}
/*
public class ApplyDamageAction : IGameAction
{
    private readonly CombatContext _ctx;

    public ApplyDamageAction(CombatContext ctx)
    {
        _ctx = ctx;
    }

    public void Execute(World world)
    {
        _ctx.Defender.HitPoints -= _ctx.FinalDamage;

        world.Enqueue(new ScriptAction(ctx2 =>
            ctx2.Notify($"{_ctx.Attacker.Name} hits {_ctx.Defender.Name} for {_ctx.FinalDamage}!")));

        if (_ctx.Defender.IsDead)
            world.Enqueue(new DeathAction(_ctx.Defender));
    }
}
*/