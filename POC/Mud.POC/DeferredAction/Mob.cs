namespace Mud.POC.DeferredAction;

public class Mob : Entity, IContainer
{
    public Room CurrentRoom { get; internal set; }
    public List<Item> Items { get; } = new();
    public List<StatusEffect.StatusEffect> StatusEffects { get; } = new();

    public string Name { get; set; }

    public bool IsNpc { get; set; }
    public NpcFlags NpcFlags { get; set; }

    public bool IsPlayer { get; set; }

    public PlayerFlags PlayerFlags { get; set; }

    public bool IsPlayerKiller =>
        IsPlayer && PlayerFlags.HasFlag(PlayerFlags.Killer);

    public bool IsThief =>
        IsPlayer && PlayerFlags.HasFlag(PlayerFlags.Thief);

    public bool IsEvil { get; set; }
    public bool IsGood { get; set; }

    public bool IsHidden { get; set; } = false;

    public Item Wielded { get; set; }

    // Core stats
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int NextLevelXP { get; set; } = 1000;
    public int BaseHitRoll { get; set; } = 0;
    public int BaseArmorClass { get; set; } = 10;

    // Hit points
    public int HitPoints { get; set; } = 50;
    public int MaxHitPoints { get; set; } = 50;

    // Mana
    public int Mana { get; set; } = 30;
    public int MaxMana { get; set; } = 30;

    // Combat
    public Mob CurrentTarget { get; set; } // null if not fighting
    public bool IsDead => Position == Position.Dead;
    public bool InCombat => CurrentTarget != null;
    public HashSet<Mob> HateList { get; } = new();

    // Charming/Group
    public bool IsCharmed { get; set; }
    public Mob Master { get; set; }
    public int Group { get; set; } = 0;

    // Position
    public Position Position { get; set; } = Position.Standing;
    // Wait
    public int Wait { get; set; } = 0; // WAIT_STATE equivalent

    public bool IsAwake =>
        Position != Position.Sleeping &&
        Position != Position.Dead;

    public bool IsConscious =>
        Position > Position.Stunned;

    // CanAct
    public bool CanAct =>
        !IsDead &&
        Position > Position.Stunned &&
        Wait == 0;

    // IContainer
    public List<Item> ItemsContainer => Items;

    // Example attack count logic (dual wield, haste, etc.)
    public int GetNumberOfAttacks()
    {
        return 1; // simple example, can extend later
    }

    public bool CanCounter(Mob attacker)
    {
        return false;
    }

    public bool IsSameGroup(Mob other)
    {
        return Group != 0 && Group == other.Group;
    }

    // Check if a mob has a skill by name
    public bool HasSkill(string skillName)
    {
        var skill = SkillBook.GetSkillByName(skillName);
        if (skill == null) return false;

        // Optional: restrict NPCs to only learn certain skills
        if (IsNpc && !NpcSkillList.Contains(skill))
            return false;

        return true;
    }

    // For NPCs, define which skills they can use
    public List<Skill> NpcSkillList { get; set; } = new();

    //
    public bool HasEffect(StatusEffectType effectType)
    {
        return StatusEffects.Any(e => e.Type == effectType);
    }

    public int GetSkillPercent(string skillName)
    {
        // For simplicity, return a fixed percentage for known skills
        if (HasSkill(skillName))
            return 75; // example fixed skill level
        return 0;
    }

    public int CheckImprove(string skillName, bool success)
    {
        // For simplicity, just return a random improvement chance
        if (!HasSkill(skillName))
            return 0;
        int chance = success ? 10 : 25; // easier to improve on failure
        int roll = Random.Shared.Next(100);
        if (roll < chance)
        {
            // In a real implementation, you'd track skill levels and improve them here
            return 1; // indicate improvement
        }
        return 0;
    }

    public void ApplyEffect(StatusEffect.StatusEffect newEffect, World world)
    {
        var existing = StatusEffects
         .FirstOrDefault(e => e.Type == newEffect.Type);

        if (existing == null)
        {
            StatusEffects.Add(newEffect);
            newEffect.Rule.OnApply(this, world);
            return;
        }

        if (newEffect.Level >= existing.Level)
        {
            existing.Refresh(newEffect.RemainingTicks, newEffect.Level);
        }
    }

    public bool IsWieldingDagger()
    {
        return Wielded != null;//TODO && Wielded.Type == ItemType.Dagger;
    }

    public void BreakStealth(World world)
    {
        bool broke = false;

        foreach (var effect in StatusEffects.ToList())
        {
            if (effect.Type == StatusEffectType.Hidden ||
                effect.Type == StatusEffectType.Sneak ||
                effect.Type == StatusEffectType.Invisibility)
            {
                StatusEffects.Remove(effect);
                broke = true;
            }
        }

        if (broke)
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{Name} reveals themselves!")));
        }
    }

    public int RollWeaponDamage()
    {
        // For simplicity, return a fixed damage value
        return 10;
    }

    public AffectModifiers GetTotalModifiers()
    {
        var total = new AffectModifiers();

        foreach (var effect in StatusEffects)
            total.Add(effect.Modifiers);

        return total;
    }

    public int GetResistance(DamageType damageType)
    {
        return 0;
    }

    public int GetSave(SaveType saveType)
    {
        return 0;
    }

    public void RemoveStealth()
    {
        StatusEffects.RemoveAll(x =>
            x.Type == StatusEffectType.Hidden ||
            x.Type == StatusEffectType.Sneak ||
            x.Type == StatusEffectType.Invisibility);
    }

    public bool IsStealthed()
    {
        return HasEffect(StatusEffectType.Hidden)
            || HasEffect(StatusEffectType.Sneak)
            || HasEffect(StatusEffectType.Invisibility);
    }
}
