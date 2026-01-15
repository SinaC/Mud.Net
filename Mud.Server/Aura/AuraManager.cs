using Microsoft.Extensions.Logging;
using Mud.Blueprints.Item;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using System.Reflection.Emit;

namespace Mud.Server.Aura;

[Export(typeof(IAuraManager)), Shared]
public class AuraManager : IAuraManager
{
    private ILogger<AuraManager> Logger { get; }
    private IAffectManager AffectManager { get; }
    private IFlagsManager FlagsManager { get; }
    private IAbilityManager AbilityManager { get; }

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

    private Dictionary<Type, FlagsAffectModifierGetterDefinition> FlagsAffectModifierGetterDefinitions { get; }

    public AuraManager(ILogger<AuraManager> logger, IAffectManager affectManager, IFlagsManager flagsManager, IAbilityManager abilityManager, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        AffectManager = affectManager;
        FlagsManager = flagsManager;
        AbilityManager = abilityManager;

        FlagsAffectModifierGetterDefinitions = [];
        var iFlagAffectGenericType = typeof(IFlagsAffect<>);
        var iFlagStringType = typeof(IFlags<string>);
        foreach (var flagsAffectType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == iFlagAffectGenericType))))
        {
            // create flags.Modifier.Get delegate
            var modifierProperty = flagsAffectType.GetProperty("Modifier");
            var modifierPropertyGetMethod = modifierProperty?.GetGetMethod();
            if (modifierProperty == null || modifierPropertyGetMethod == null)
                Logger.LogError("AuraManager: Modifier property getter not found in FlagAffect {flagAffectType}", flagsAffectType);
            else
            {
                // https://titiandragomir.wordpress.com/2009/12/22/getting-and-setting-property-values-dynamically/
                // Create delegate to Getter
                DynamicMethod dmGet = new DynamicMethod("Get", typeof(IFlags<string>), [typeof(object),]);
                ILGenerator ilGet = dmGet.GetILGenerator();
                // Load first argument to the stack
                ilGet.Emit(OpCodes.Ldarg_0);
                // Cast the object on the stack to the apropriate type
                ilGet.Emit(OpCodes.Castclass, flagsAffectType);
                // Call the getter method passing the object on the stack as the this reference
                ilGet.Emit(OpCodes.Callvirt, modifierPropertyGetMethod);
                // If the property type is a value type (int/DateTime/..)
                // box the value so we can return it
                if (modifierProperty.PropertyType.IsValueType)
                {
                    ilGet.Emit(OpCodes.Box, modifierProperty.PropertyType);
                }
                // Return from the method
                ilGet.Emit(OpCodes.Ret);
                // Create getter delegate
                var getter = (Func<object, IFlags<string>>)dmGet.CreateDelegate(typeof(Func<object, IFlags<string>>));

                var flagsAffectModifierGetterDefinition = new FlagsAffectModifierGetterDefinition { FlagsAffectType = flagsAffectType, IFlagsAffectType = modifierPropertyGetMethod.ReturnType, ModifierFlagsGetterFunc = getter };
                FlagsAffectModifierGetterDefinitions.Add(flagsAffectType, flagsAffectModifierGetterDefinition);
            }
        }
    }

    public IAura AddAura(IEntity target, string abilityName, IEntity source, int level, TimeSpan duration, AuraFlags auraFlags, bool recompute, params IAffect?[]? affects)
    {
        var abilityDefinition = AbilityManager[abilityName];
        var aura = new Aura(abilityDefinition, source, auraFlags, level, duration, affects);
        CheckAffects(affects);
        target.AddAura(aura, recompute);
        return aura;
    }

    public IAura AddAura(IEntity target, string abilityName, IEntity source, int level, AuraFlags auraFlags, bool recompute, params IAffect?[]? affects)
    {
        var abilityDefinition = AbilityManager[abilityName];
        var aura = new Aura(abilityDefinition, source, auraFlags, level, affects);
        CheckAffects(affects);
        target.AddAura(aura, recompute);
        return aura;
    }

    public IAura AddAura(IEntity target, IEntity source, int level, AuraFlags auraFlags, bool recompute, params IAffect?[]? affects)
    {
        var aura = new Aura(null, source, auraFlags | AuraFlags.Inherent, level, affects);
        CheckAffects(affects);
        target.AddAura(aura, recompute);
        return aura;
    }

    public IAura AddAura(IEntity target, AuraData auraData, bool recompute)
    {
        var aura = new Aura(AffectManager, AbilityManager, auraData);
        // TODO: how could we check if an affecs is IFlagsAffect and if a value found in Modifier is invalid using FlagsManager
        target.AddAura(aura, recompute);
        return aura;
    }

    private bool CheckAffects(IAffect?[]? affects)
    {
        if (affects == null)
            return true;
        foreach (var affect in affects)
        {
            if (affect is not null)
            {
                // if affect is derived from FlagsAffectBase<TFlags>, check Modifier flags values
                var affectType = affect.GetType();
                if (FlagsAffectModifierGetterDefinitions.TryGetValue(affectType, out var flagsAffectModifierGetterDefinition))
                {
                    var flags = flagsAffectModifierGetterDefinition.ModifierFlagsGetterFunc(affect);
                    FlagsManager.CheckFlags(flagsAffectModifierGetterDefinition.IFlagsAffectType, flags);
                }
            }
        }
        return true;
    }

    private class FlagsAffectModifierGetterDefinition
    {
        public required Type FlagsAffectType { get; init; }
        public required Type IFlagsAffectType { get; init; }
        public required Func<object, IFlags<string>> ModifierFlagsGetterFunc { get; init; }
    }
}
