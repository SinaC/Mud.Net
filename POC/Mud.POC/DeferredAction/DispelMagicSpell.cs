using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class DispelMagicSpell : Skill
{
    public DispelMagicSpell()
        : base("dispel magic", manaCost: 20, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        foreach (var effect in target.StatusEffects.ToList())
        {
            if (!effect.Dispellable)
                continue;

            int chance = 50 + (caster.Level - effect.Level) * 5;

            if (Random.Shared.Next(100) < chance)
            {
                effect.Expire(target, world);
                target.StatusEffects.Remove(effect);

                world.Enqueue(new ScriptAction(ctx =>
                    ctx.Notify($"{effect.Type} is dispelled!")));
            }
        }
    }

    //public override void Execute(Mob caster, Mob target, World world)
    //{
    //    foreach (var effect in target.StatusEffects.ToList())
    //    {
    //        if (!effect.IsMagical)
    //            continue;

    //        int chance = 50 + (caster.Level - effect.Source?.Level ?? 0) * 5;
    //        int roll = Random.Shared.Next(100);

    //        if (roll < chance)
    //        {
    //            target.StatusEffects.Remove(effect);

    //            world.Enqueue(new ScriptAction(ctx =>
    //                ctx.Notify($"{effect.Type} is dispelled from {target.Name}!")));
    //        }
    //    }
}
//public class DispelMagicSpell : Skill
//{
//    public DispelMagicSpell() : base("dispel", 15, 2) { }

//    public override void Execute(Mob caster, Mob target, World world)
//    {
//        foreach (var affect in target.ActiveAffects.ToList())
//        {
//            if (!affect.IsSpell)
//                continue;

//            if (Random.Shared.Next(100) < 75)
//            {
//                target.ActiveAffects.Remove(affect);
//                target.Affects &= ~affect.Flags;
//            }
//        }
//    }
//}
