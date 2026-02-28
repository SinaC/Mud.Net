using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class FaerieFireSpell : Skill
{
    public FaerieFireSpell()
        : base("faerie fire", manaCost: 15, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (target == null) return;

        target.StatusEffects.RemoveAll(e =>
            e.Type == StatusEffectType.Invisibility ||
            e.Type == StatusEffectType.Hidden);

        target.ApplyEffect(new StatusEffect.StatusEffect(
            Name,
            StatusEffectType.FaerieFire,
            10,
            caster.Level,
            new AffectModifiers
            {
                ArmorClassBonus = +100, // worse AC
            },
            new FaerieFireEffectRule()), world);

        //world.Enqueue(new ScriptAction(ctx =>
        //    ctx.Notify($"{target.Name} is surrounded by a pink aura!")));
    }
}
