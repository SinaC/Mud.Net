using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Aura;

[Export(typeof(IAuraManager)), Shared]
public class AuraManager : IAuraManager
{
    private IAffectManager AffectManager { get; }

    //public IPeriodicAura AddPeriodicAura(IEntity target, IAbility ability, IEntity source, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
    //{
    //    IPeriodicAura periodicAura = new PeriodicAura(ability, PeriodicAuraTypes.Heal, source, amount, amountOperator, level, tickVisible, tickDelay, totalTicks);
    //    target.AddPeriodicAura(periodicAura);
    //    return periodicAura;
    //}

    //public IPeriodicAura AddPeriodicAura(IEntity target, IAbility ability, IEntity source, SchoolTypes school, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
    //{
    //    IPeriodicAura periodicAura = new PeriodicAura(ability, PeriodicAuraTypes.Damage, source, school, amount, amountOperator, level, tickVisible, tickDelay, totalTicks);
    //    target.AddPeriodicAura(periodicAura);
    //    return periodicAura;
    //}

    public AuraManager(IAffectManager affectManager)
    {
        AffectManager = affectManager;
    }

    public IAura AddAura(IEntity target, string abilityName, IEntity source, int level, TimeSpan duration, AuraFlags auraFlags, bool recompute, params IAffect?[]? affects)
    {
        var aura = new Aura(abilityName, source, auraFlags, level, duration, affects);
        target.AddAura(aura, recompute);
        return aura;
    }

    public IAura AddAura(IEntity target, string abilityName, IEntity source, int level, AuraFlags auraFlags, bool recompute, params IAffect?[]? affects)
    {
        var aura = new Aura(abilityName, source, auraFlags, level, affects);
        target.AddAura(aura, recompute);
        return aura;
    }

    public IAura AddAura(IEntity target, AuraData auraData, bool recompute)
    {
        var aura = new Aura(AffectManager, auraData);
        target.AddAura(aura, recompute);
        return aura;
    }
}
