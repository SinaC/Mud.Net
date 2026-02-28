using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class SkillAction : IGameAction
{
    private readonly Mob _user;
    private readonly Skill _skill;
    private readonly Mob _target;

    public SkillAction(Mob user, Skill skill, Mob target)
    {
        _user = user;
        _skill = skill;
        _target = target;
    }

    public void Execute(World world)
    {
        if (_user.IsDead) return;
        if (!_user.HasSkill(_skill.Name)) return;

        // Check cooldown
        if (_skill.IsOnCooldown(_user, world))
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{_skill.Name} is on cooldown for {_user.Name}!")));
            return;
        }

        // For active skills like steal/disarm/rescue that may not use mana, set wait
        if (_skill.ManaCost > 0 && _user.Mana < _skill.ManaCost)
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{_user.Name} does not have enough mana for {_skill.Name}!")));
            return;
        }

        // Deduct mana and set last used tick
        if (_skill.ManaCost > 0)
            _user.Mana -= _skill.ManaCost;

        _skill.Use(_user, _target, world);
    }
}