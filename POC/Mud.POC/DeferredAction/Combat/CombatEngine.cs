using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class CombatEngine
{
    private readonly StatusEffectEngine _statusEffectEngine;
    private readonly List<ICombatRule> _preHitRules;
    private readonly List<ICombatRule> _postHitRules;
    private readonly List<ICombatRule> _damageRules;

    public CombatEngine(
        StatusEffectEngine statusEffectEngine,
        IEnumerable<ICombatRule> preHit,
        IEnumerable<ICombatRule> postHit,
        IEnumerable<ICombatRule> damage)
    {
        _statusEffectEngine = statusEffectEngine;
        _preHitRules = preHit.ToList();
        _postHitRules = postHit.ToList();
        _damageRules = damage.ToList();
    }

    public CombatContext Resolve(Mob attacker, Mob defender, bool backstab = false)
    {
        //var baseDamage = CombatFormulas.RollDamage(attacker, defender);
        var baseDamage = attacker.RollWeaponDamage();

        var ctx = new CombatContext(attacker, defender)
        {
            BaseDamage = baseDamage,
            IsBackstab = backstab
        };

        foreach (var rule in _preHitRules)
            rule.Apply(ctx);

        foreach (var rule in _postHitRules)
            rule.Apply(ctx);

        if (!ctx.IsHit)
            return ctx;

        ctx.ModifiedDamage = ctx.BaseDamage;

        foreach (var rule in _damageRules)
            rule.Apply(ctx);

        _statusEffectEngine.ApplyCombatModifiers(ctx);

        ctx.FinalDamage = Math.Max(0, ctx.ModifiedDamage);

        return ctx;
    }

    public CombatContext ResolveSpell( Mob caster, Mob target, Skill spell, int baseDamage, SaveType saveType)
    {
        var ctx = new CombatContext(caster, target)
        {
            IsSpell = true,
            Spell = spell,
            BaseDamage = baseDamage,
            DamageType = spell.DamageType,
            SaveType = saveType
        };

        ctx.ModifiedDamage = ctx.BaseDamage;

        foreach (var rule in _damageRules)
            rule.Apply(ctx);

        ctx.FinalDamage = Math.Max(0, ctx.ModifiedDamage);

        return ctx;
    }
}