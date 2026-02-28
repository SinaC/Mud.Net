using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class StatusEffect
{
    public string Name { get; }
    public StatusEffectType Type { get; }
    public int RemainingTicks { get; private set; }
    public int Level { get; }


    public bool Dispellable { get; }
    public AffectModifiers Modifiers { get; }

    public IStatusEffectRule Rule { get; }

    public bool IsExpired => RemainingTicks <= 0;

    public StatusEffect(
        string name,
        StatusEffectType type,
        int duration,
        int level,
        AffectModifiers modifiers,
        IStatusEffectRule rule,
        bool dispellable = true)
    {
        Name = name;
        Type = type;
        RemainingTicks = duration;
        Level = level;
        Modifiers = modifiers ?? new AffectModifiers();
        Rule = rule;
        Dispellable = dispellable;
    }

    public void Tick(Mob target, World world)
    {
        Rule.OnTick(target, world);

        if (RemainingTicks > 0)
            RemainingTicks--;
    }

    public void Expire(Mob target, World world)
    {
        Rule.OnRemove(target, world);
    }

    public void Refresh(int duration, int level)
    {
        RemainingTicks = Math.Max(RemainingTicks, duration);
    }
}
