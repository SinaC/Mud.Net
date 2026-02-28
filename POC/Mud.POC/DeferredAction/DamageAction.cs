using Mud.POC.DeferredAction.Combat;

namespace Mud.POC.DeferredAction;

//Purpose: Simple “raw damage” delivery.
//Behavior:
//  Subtracts hit points directly from the target.
//  Optionally triggers death if HP ≤ 0.
//  Usually doesn’t account for status effects, sanctuaries, armor modifiers, or saving throws.
//Use case:
//  Quick or low - level attacks.
//  Internal calculations that are already adjusted for hitroll, AC, resistance.
public class DamageAction : IGameAction
{
    private readonly Mob _attacker;
    private readonly Mob _target;
    private readonly int _amount;

    public DamageAction(Mob attacker, Mob target, int amount)
    {
        _attacker = attacker;
        _target = target;
        _amount = amount;
    }

    public void Execute(World world)
    {
        if (_target.IsDead) return;

        // Subtract HP
        _target.HitPoints -= _amount;

        // Break sneak/hide on attack
        if (_attacker != null && _amount > 0)
        {
            _target.BreakStealth(world);
        }

        if (_target.HitPoints <= -11)
            _target.Position = Position.Dead;
        else if (_target.HitPoints <= -6)
            _target.Position = Position.Mortal;
        else if (_target.HitPoints <= -3)
            _target.Position = Position.Incap;
        else if (_target.HitPoints <= 0)
            _target.Position = Position.Stunned;
        else if (_target.InCombat)
            _target.Position = Position.Fighting;

        // Notify
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{_attacker?.Name ?? "Someone"} hits {_target.Name} for {_amount} damage!")));

        // Check death
        if (_target.IsDead)
        {
            world.Enqueue(new DeathAction(_target));
        }
    }
}
/*public class DamageAction : IGameAction
{
    private readonly Mob _attacker;
    private readonly Mob _defender;
    private readonly CombatEngine _engine;
    private readonly bool _backstab;

    public DamageAction(Mob attacker, Mob defender, CombatEngine combatEngine, bool backstab = false)
    {
        _attacker = attacker;
        _defender = defender;
        _engine = combatEngine;
        _backstab = backstab;
    }

    public void Execute(World world)
    {
        var ctx = _engine.Resolve(_attacker, _defender, _backstab);

        if (!ctx.IsHit)
            return;

        var damage = ctx.FinalDamage;
        _defender.HitPoints -= damage;

        if (_defender.HitPoints <= -11)
            _defender.Position = Position.Dead;
        else if (_defender.HitPoints <= -6)
            _defender.Position = Position.Mortal;
        else if (_defender.HitPoints <= -3)
            _defender.Position = Position.Incap;
        else if (_defender.HitPoints <= 0)
            _defender.Position = Position.Stunned;
        else if (_defender.InCombat)
            _defender.Position = Position.Fighting;

        // Notify players (in a real MUD this would go to clients)
        world.Enqueue(new ScriptAction(ctx =>
        {
            string attackerName = _attacker?.Name ?? "The environment";
            ctx.Notify($"{_defender.Name} takes {damage} damage from {attackerName}!");
        }));

        if (_defender.IsDead)
            world.Enqueue(new DeathAction(_defender));
    }
}
*/
/*
public class DamageAction : IGameAction
{
    private readonly Mob _attacker; // Can be null for environment/status effects
    private readonly Mob _target;
    private readonly int _amount;

    public DamageAction(Mob attacker, Mob target, int amount)
    {
        _attacker = attacker;
        _target = target;
        _amount = amount;
    }

    public void Execute(World world)
    {
        if (_target.IsDead) return;

        int damage = _amount;

        // Apply sanctuary/protection modifiers
        if (StatusEffect.Has(_target, "sanctuary"))
            damage /= 2;

        if (StatusEffect.Has(_target, "protection"))
            damage = (int)(damage * 0.8); // 20% damage reduction

        // Apply positional modifiers (e.g., sitting/resting reduces damage)
        if (_target.Position == Position.Resting)
            damage = (int)(damage * 0.9);
        else if (_target.Position == Position.Sitting)
            damage = (int)(damage * 0.95);

        _target.HitPoints -= damage;

        if (_target.HitPoints <= -11)
            _target.Position = Position.Dead;
        else if (_target.HitPoints <= -6)
            _target.Position = Position.Mortal;
        else if (_target.HitPoints <= -3)
            _target.Position = Position.Incap;
        else if (_target.HitPoints <= 0)
            _target.Position = Position.Stunned;
        else if (_target.InCombat)
            _target.Position = Position.Fighting;

        // Notify players (in a real MUD this would go to clients)
        world.Enqueue(new ScriptAction(ctx =>
        {
            string attackerName = _attacker?.Name ?? "The environment";
            ctx.Notify($"{_target.Name} takes {damage} damage from {attackerName}!");
        }));

        if (_target.IsDead)
            world.Enqueue(new DeathAction(_target));
    }
}
*/
//public class DamageAction : IGameAction
//{
//    private readonly Mob _attacker;
//    private readonly Mob _target;
//    private readonly int _amount;

//    public DamageAction(Mob attacker, Mob target, int amount)
//    {
//        _attacker = attacker;
//        _target = target;
//        _amount = amount;
//    }

//    public void Execute(World world)
//    {
//        if (_target.Position == Position.Dead) return;

//        int damage = _amount;

//        // Sanctuary
//        if (_target.Affects.HasFlag(AffectFlags.Sanctuary))
//            damage /= 2;

//        // Protection logic
//        if (_target.Affects.HasFlag(AffectFlags.ProtectionEvil) && _attacker.IsEvil)
//            damage = (int)(damage * 0.75);

//        if (_target.Affects.HasFlag(AffectFlags.ProtectionGood) && _attacker.IsGood)
//            damage = (int)(damage * 0.75);

//        _target.HitPoints -= _amount;

//        if (_target.HitPoints <= -11)
//        {
//            _target.Position = Position.Dead;
//            world.Enqueue(new DeathAction(_target));
//            return;
//        }

//        if (_target.HitPoints <= -6)
//            _target.Position = Position.Mortal;
//        else if (_target.HitPoints <= -3)
//            _target.Position = Position.Incap;
//        else if (_target.HitPoints <= 0)
//            _target.Position = Position.Stunned;
//        else if (_target.InCombat)
//            _target.Position = Position.Fighting;
//    }
//}
