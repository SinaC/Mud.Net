namespace Mud.POC.DeferredAction;

public class AttackAction : IGameAction
{
    private readonly Mob _attacker;
    private readonly Mob _target;

    public AttackAction(Mob attacker, Mob target)
    {
        _attacker = attacker;
        _target = target;
    }

    public void Execute(World world)
    {
        if (!_attacker.CanAct || _target.IsDead)
            return;

        var ctx = world.CombatEngine.Resolve(_attacker, _target);

        if (!ctx.IsHit)
        {
            world.Enqueue(new ScriptAction(ctx2 =>
                ctx2.Notify($"{_attacker.Name} misses {_target.Name}.")));
        }

        world.CheckKiller(_attacker, _target);

        world.Enqueue(new DamageAction(_attacker, _target, ctx.FinalDamage));

        world.RecordAttack(_attacker, _target);

        world.Enqueue(new AssistAction(_target, _attacker));

        if (_target.CanCounter(_attacker))
            world.Enqueue(new AttackAction(_target, _attacker));
    }
}