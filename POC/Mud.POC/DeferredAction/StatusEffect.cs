using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;
/*
public class StatusEffect
{
    public string Name { get; }
    public StatusEffectType Type { get; }
    public int RemainingTicks { get; private set; }
    public AffectModifiers Modifiers { get; set; } = new();
    public bool IsMagical { get; set; } = true;

    // Optional: effect source (who applied it)
    public Mob Source { get; }

    // Metadata for skill-specific info, damage modifiers, or flags
    public Dictionary<string, object> Metadata { get; } = new();

    public StatusEffect(string name, StatusEffectType type, int durationTicks, Mob source = null)
    {
        Name = name;
        Type = type;
        RemainingTicks = durationTicks;
        Source = source;
    }

    /// <summary>
    /// Called each tick to apply effect and reduce duration
    /// </summary>
    public void Tick(Mob mob, World world)
    {
        if (RemainingTicks <= 0) return;

        // Apply periodic effect (poison, bleeding, etc.)
        ApplyEffect(mob, world);

        RemainingTicks--;

        if (RemainingTicks <= 0)
            Expire(mob, world);
    }

    /// <summary>
    /// Apply effect logic each tick
    /// </summary>
    protected virtual void ApplyEffect(Mob mob, World world)
    {
        switch (Name.ToLower())
        {
            case "poison":
                int damage = Metadata.ContainsKey("damagePerTick")
                    ? (int)Metadata["damagePerTick"]
                    : 5;

                // DamageAction now expects attacker, target, damage
                world.Enqueue(new DamageAction(Source, mob, damage));

                world.Enqueue(new ScriptAction(ctx =>
                    ctx.Notify($"{mob.Name} suffers {damage} poison damage!")));
                break;

            case "bleeding":
                int bleed = Metadata.ContainsKey("damagePerTick")
                    ? (int)Metadata["damagePerTick"]
                    : 3;

                world.Enqueue(new DamageAction(Source, mob, bleed));
                world.Enqueue(new ScriptAction(ctx =>
                    ctx.Notify($"{mob.Name} bleeds for {bleed} damage!")));
                break;
        }
    }

    /// <summary>
    /// Called when effect expires
    /// </summary>
    protected virtual void Expire(Mob mob, World world)
    {
        mob.StatusEffects.Remove(this);

        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{mob.Name}'s {Name} effect has worn off.")));
    }

    /// <summary>
    /// Helper: check if mob has a specific status effect
    /// </summary>
    public static bool Has(Mob mob, string effectName)
        => mob.StatusEffects.Any(e => e.Name.Equals(effectName, StringComparison.OrdinalIgnoreCase));

    public static void Remove(Mob mob, string effectName)
    {
        mob.StatusEffects.RemoveAll(e => e.Name.Equals(effectName, StringComparison.OrdinalIgnoreCase));
    }
}
*/