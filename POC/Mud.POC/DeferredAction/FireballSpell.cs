using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class FireballSpell : Skill
{
    public FireballSpell() : base("Fireball", 10, cooldownTicks: 2)
    {
    }

    public override void Execute(Mob caster, Mob target, World world)
    {
        int baseDamage = Dice.Roll(caster.Level, 6);

        var ctx = world.CombatEngine.ResolveSpell(
            caster,
            target,
            SkillBook.GetSkillByName("Fireball"),
            baseDamage,
            SaveType.Spell);

        world.Enqueue(new DamageAction(caster, target, ctx.FinalDamage));

        // TODO
        //        world.Enqueue(new ScriptAction(ctx =>
        //        {
        //            ctx.Notify($"{caster} casts Fireball, hitting {targets.Count} targets for {_damage} damage each!");
        //        }));
    }
}
//public class FireballSpell : Skill
//{
//    private readonly int _damage;

//    public FireballSpell() : base("Fireball", 10, cooldownTicks: 2)
//    {
//        _damage = 40;
//    }

//    public override void Execute(Mob caster, Mob target, World world)
//    {
//        var targets = caster.CurrentRoom.Mobs.Where(m => m != caster && !m.IsDead).ToList();
//        foreach (var t in targets)
//        {
//            world.Enqueue(new DamageAction(caster, t, _damage));
//        }

//        world.Enqueue(new ScriptAction(ctx =>
//        {
//            ctx.Notify($"{caster} casts Fireball, hitting {targets.Count} targets for {_damage} damage each!");
//        }));
//    }
//}
