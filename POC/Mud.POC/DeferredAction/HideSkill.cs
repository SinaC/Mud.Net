using Mud.POC.DeferredAction.StatusEffect;

namespace Mud.POC.DeferredAction;

public class HideSkill : Skill
{
    public HideSkill() : base("hide", manaCost: 0, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (caster.InCombat)
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You cannot hide while fighting.")));
            return;
        }

        caster.StatusEffects.RemoveAll(e => e.Type == StatusEffectType.Hidden);

        int skill = caster.GetSkillPercent("hide");
        int roll = Random.Shared.Next(100);

        if (roll < skill)
        {
            caster.ApplyEffect(new StatusEffect.StatusEffect(
                "Hide",
                StatusEffectType.Hidden,
                -1, // infinite until broken
                caster.Level,
                null,
                new HideEffectRule()), world);

            caster.CheckImprove("hide", true);
        }
        else
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You fail to hide.")));

            caster.CheckImprove("hide", false);
        }

        caster.Wait = 2;
    }
}

//public class HideSkill : Skill
//{
//    public HideSkill() : base("hide", 0, 0) { }

//    public override void Execute(Mob caster, Mob target, World world)
//    {
//        caster.Wait = 1;
//        caster.StatusEffects.Add(new StatusEffect("Hidden", 5)); // lasts 5 ticks
//        world.Enqueue(new ScriptAction(ctx =>
//            ctx.Notify($"{caster.Name} hides in the shadows.")));
//    }
//}