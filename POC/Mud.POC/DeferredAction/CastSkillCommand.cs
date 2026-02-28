namespace Mud.POC.DeferredAction;

public class CastSkillCommand : PlayerCommand
{
    private readonly Mob _caster;
    private readonly Skill _skill;
    private readonly Mob _target;

    public CastSkillCommand(Mob caster, Skill skill, Mob target)
    {
        _caster = caster;
        _skill = skill;
        _target = target;
    }

    public override void Execute(Mob player, World world)
    {
        if (_caster.IsDead) return;
        if (_caster.Mana < _skill.ManaCost)
        {
            world.Enqueue(new ScriptAction(ctx => ctx.Notify($"{_caster} does not have enough mana for {_skill.Name}!")));
            return;
        }

        if (_skill.IsOnCooldown(_caster, world))
        {
            world.Enqueue(new ScriptAction(ctx => ctx.Notify($"{_skill.Name} is on cooldown!")));
            return;
        }

        _skill.Use(_caster, _target, world);
    }
}