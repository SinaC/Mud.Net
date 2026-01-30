using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Spell;

public abstract class SpellBase : ISpell
{
    protected ILogger<SpellBase> Logger { get; }
    protected IRandomManager RandomManager { get; }

    protected bool IsSetupExecuted { get; private set; }
    protected bool IsCastFromItem { get; private set; }
    protected IAbilityDefinition AbilityDefinition { get; private set; } = default!;
    protected ICharacter Caster { get; private set; } = default!;
    protected int Level { get; private set; }
    protected ResourceCostToPay[] ResourceCostsToPay { get; private set; } = default!;

    protected SpellBase(ILogger<SpellBase> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    #region ISpell

    public virtual string? Setup(ISpellActionInput spellActionInput)
    {
        IsSetupExecuted = true;

        if (spellActionInput.IsCastFromItem)
            return SetupFromItem(spellActionInput);
        else
            return SetupFromCast(spellActionInput);
    }

    public virtual void Execute()
    {
        if (!IsSetupExecuted)
            throw new Exception("Cannot execute spell without calling setup first.");
        if (IsCastFromItem)
            ExecuteFromItem();
        else
            ExecuteFromCast();
    }

    #endregion

    protected abstract string? SetTargets(ISpellActionInput spellActionInput);
    protected abstract void Invoke();

    private string? SetupFromCast(ISpellActionInput spellActionInput)
    {
        IsCastFromItem = false;
        AbilityDefinition = spellActionInput.AbilityDefinition;

        // 1) check caster
        Caster = spellActionInput.Caster;
        if (Caster == null)
            return "Spell must be cast by a character.";
        if (Caster.Room == null)
            return "You are nowhere...";
        Level = spellActionInput.Level;

        // 2) check guards
        if (AbilityDefinition.Guards.Length > 0)
        {
            foreach (var guard in AbilityDefinition.Guards)
            {
                var guardResult = guard.Guards(Caster, spellActionInput.Parameters);
                if (guardResult != null)
                    return guardResult;
            }
        }

        // 3) check targets
        var setTargetResult = SetTargets(spellActionInput);
        if (setTargetResult != null)
            return setTargetResult;

        // 4) check cooldown
        int cooldownPulseLeft = Caster.CooldownPulseLeft(AbilityDefinition.Name);
        if (cooldownPulseLeft > 0)
            return $"{AbilityDefinition.Name} is in cooldown for {Pulse.ToTimeSpan(cooldownPulseLeft).FormatDelay()}.";

        // 5) check resource cost (even if in Infinite immortal mode, we calculate resource to pay in case abilities would use that information)
        var (_, abilityLearned) = Caster.GetAbilityLearnedAndPercentage(AbilityDefinition.Name);
        if (abilityLearned != null && abilityLearned.HasCost)
        {
            var resourceCostToPays = new List<ResourceCostToPay>();
            foreach (var abilityResourceCost in abilityLearned.AbilityUsage.ResourceCosts)
            {
                var resourceKind = abilityResourceCost.ResourceKind;
                if (!Caster.CurrentResourceKinds.Contains(resourceKind) && !Caster.ImmortalMode.IsSet("Infinite")) // TODO: not sure about this test
                    return $"You can't use {resourceKind} as resource for the moment.";
                int resourceLeft = Caster[resourceKind];
                int cost;
                bool isAll = false;
                switch (abilityResourceCost.CostAmountOperator)
                {
                    case CostAmountOperators.Fixed:
                        cost = abilityResourceCost.CostAmount;
                        break;
                    case CostAmountOperators.PercentageCurrent:
                        cost = Caster[resourceKind] * abilityResourceCost.CostAmount / 100;
                        break;
                    case CostAmountOperators.PercentageMax:
                        cost = Caster.MaxResource(resourceKind) * abilityResourceCost.CostAmount / 100;
                        break;
                    case CostAmountOperators.All:
                        cost = Caster[resourceKind];
                        isAll = true;
                        break;
                    case CostAmountOperators.AllWithMin:
                        cost = Math.Max(abilityResourceCost.CostAmount, Caster[resourceKind]);
                        isAll = true;
                        break;

                    default:
                        Logger.LogError("Unexpected CostAmountOperator {costAmountOperator} for ability {abilityName}.", abilityResourceCost.CostAmountOperator, AbilityDefinition.Name);
                        cost = 100;
                        break;
                }
                bool enoughResource = cost <= resourceLeft;
                if (!enoughResource && !Caster.ImmortalMode.IsSet("Infinite"))
                    return $"You don't have enough {resourceKind.DisplayName()}.";
                var resourceCostToPay = new ResourceCostToPay(resourceKind, cost, isAll);
                resourceCostToPays.Add(resourceCostToPay);
            }
            ResourceCostsToPay = resourceCostToPays.ToArray();
        }
        else
        {
            ResourceCostsToPay = [];
        }
        return null;
    }

    private string? SetupFromItem(ISpellActionInput spellActionInput)
    {
        IsCastFromItem = true;
        AbilityDefinition = spellActionInput.AbilityDefinition;

        // 1) check caster
        Caster = spellActionInput.Caster;
        if (Caster == null)
            return "Spell must be cast by a character.";
        if (Caster.Room == null)
            return "You are nowhere...";
        Level = spellActionInput.Level;

        ResourceCostsToPay = []; // no resource to pay when cast from item

        // 2) check targets
        var setTargetResult = SetTargets(spellActionInput);
        if (setTargetResult != null)
            return setTargetResult;
        return null;
    }

    private void ExecuteFromCast()
    {
        var pcCaster = Caster as IPlayableCharacter;

        // 1) check if failed
        var (percentage, abilityLearned) = Caster.GetAbilityLearnedAndPercentage(AbilityDefinition.Name);
        if (abilityLearned != null && !RandomManager.Chance(percentage))
        {
            Caster.Send(StringHelpers.YouLostYourConcentration);
            pcCaster?.CheckAbilityImprove(AbilityDefinition.Name, false, 1);
            // pay half cost except if 'all'
            if (!Caster.ImmortalMode.IsSet("Infinite"))
            {
                foreach (var resourceCostToPay in ResourceCostsToPay.Where(x => x.CostAmount > 0))
                {
                    if (resourceCostToPay.IsAll)
                        Caster.UpdateResource(resourceCostToPay.ResourceKind, -resourceCostToPay.CostAmount);
                    else
                        Caster.UpdateResource(resourceCostToPay.ResourceKind, -resourceCostToPay.CostAmount / 2);
                }
            }
            return;
        }

        // 2) pay costs
        if (!Caster.ImmortalMode.IsSet("Infinite"))
        {
            foreach (var resourceCostToPay in ResourceCostsToPay.Where(x => x.CostAmount > 0))
            {
                Caster.UpdateResource(resourceCostToPay.ResourceKind, -resourceCostToPay.CostAmount);
            }
        }

        // 3) say spell if not ventriloquate
        SaySpell();

        // 4) invoke spell
        Invoke();

        // 5) GCD
        if (AbilityDefinition.PulseWaitTime != null)
            Caster.SetGlobalCooldown(AbilityDefinition.PulseWaitTime.Value);

        // 6) set cooldown if any
        if (AbilityDefinition.CooldownInSeconds != null && AbilityDefinition.CooldownInSeconds.Value > 0)
            Caster.SetCooldown(AbilityDefinition.Name, TimeSpan.FromSeconds(AbilityDefinition.CooldownInSeconds.Value));

        // 7) check improve true
        pcCaster?.CheckAbilityImprove(AbilityDefinition.Name, true, AbilityDefinition.LearnDifficultyMultiplier);
    }

    private void ExecuteFromItem()
    {
        Invoke();
    }

    protected virtual void SaySpell()
        => SpellHelper.SaySpell(Caster, AbilityDefinition);

    protected record ResourceCostToPay(ResourceKinds ResourceKind, int CostAmount, bool IsAll);
}
