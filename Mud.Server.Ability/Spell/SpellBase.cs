using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class SpellBase : ISpell
{
    protected ILogger<SpellBase> Logger { get; }
    protected IRandomManager RandomManager { get; }
    protected IAbilityDefinition AbilityDefinition { get; }

    protected bool IsSetupExecuted { get; private set; }
    protected ICharacter Caster { get; private set; } = default!;
    protected int Level { get; private set; }
    protected int? Cost { get; private set; }
    protected ResourceKinds? ResourceKind { get; private set; }
    protected bool IsCastFromItem { get; private set; }

    protected SpellBase(ILogger<SpellBase> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
        AbilityDefinition = new AbilityDefinition(GetType());
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

        // 1) check caster
        Caster = spellActionInput.Caster;
        if (Caster == null)
            return "Spell must be cast by a character.";
        if (Caster.Room == null)
            return "You are nowhere...";
        Level = spellActionInput.Level;

        // 2) check shape
        if (AbilityDefinition.AllowedShapes?.Length > 0 && !AbilityDefinition.AllowedShapes.Contains(Caster.Shape))
        {
            if (AbilityDefinition.AllowedShapes.Length > 1)
                return $"You are not in {string.Join(" or ", AbilityDefinition.AllowedShapes.Select(x => x.ToString()))} shape";
            return $"You are not in {AbilityDefinition.AllowedShapes.Single()} shape";
        }

        // 3) check position && not in combat
        if (AbilityDefinition.MinPosition is not null && (Caster.Position < AbilityDefinition.MinPosition || (AbilityDefinition.NotInCombat == true && Caster.Fighting != null)))
            return "You can't concentrate enough.";

        // 4) check targets
        var setTargetResult = SetTargets(spellActionInput);
        if (setTargetResult != null)
            return setTargetResult;

        // 5) check cooldown
        int cooldownPulseLeft = Caster.CooldownPulseLeft(AbilityDefinition.Name);
        if (cooldownPulseLeft > 0)
            return $"{AbilityDefinition.Name} is in cooldown for {(cooldownPulseLeft / Pulse.PulsePerSeconds).FormatDelay()}.";

        // 6) check resource cost
        var (_, abilityLearned) = Caster.GetAbilityLearnedAndPercentage(AbilityDefinition.Name);
        if (abilityLearned != null && abilityLearned.HasCost())
        {
            var resourceKind = abilityLearned.ResourceKind!.Value;
            if (!Caster.CurrentResourceKinds.Contains(resourceKind)) // TODO: not sure about this test
                return $"You can't use {resourceKind} as resource for the moment.";
            int resourceLeft = Caster[resourceKind];
            int cost;
            switch (abilityLearned.CostAmountOperator)
            {
                case CostAmountOperators.Fixed:
                    cost = abilityLearned.CostAmount;
                    break;
                case CostAmountOperators.Percentage:
                    cost = Caster.MaxResource(resourceKind) * abilityLearned.CostAmount / 100;
                    break;
                case CostAmountOperators.All:
                    cost = Math.Min(abilityLearned.CostAmount, Caster[resourceKind]);
                    break;
                default:
                    Logger.LogError("Unexpected CostAmountOperator {costAmountOperator} for ability {abilityName}.", abilityLearned.CostAmountOperator, AbilityDefinition.Name);
                    cost = 100;
                    break;
            }
            var enoughResource = cost <= resourceLeft;
            if (!enoughResource)
                return $"You don't have enough {resourceKind}.";
            Cost = cost;
            ResourceKind = resourceKind;
        }
        else
        {
            Cost = null;
            ResourceKind = null;
        }
        return null;
    }

    private string? SetupFromItem(ISpellActionInput spellActionInput)
    {
        IsCastFromItem = true;

        // 1) check caster
        Caster = spellActionInput.Caster;
        if (Caster == null)
            return "Spell must be cast by a character.";
        if (Caster.Room == null)
            return "You are nowhere...";
        Level = spellActionInput.Level;

        Cost = null;
        ResourceKind = null;

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
            Caster.Send("You lost your concentration.");
            pcCaster?.CheckAbilityImprove(AbilityDefinition.Name, false, 1);
            // pay half resource
            if (Cost.HasValue && Cost.Value > 1 && ResourceKind.HasValue)
                Caster.UpdateResource(ResourceKind.Value, -Cost.Value / 2);
            return;
        }

        // 2) pay resource
        if (Cost.HasValue && Cost.Value >= 1 && ResourceKind.HasValue)
            Caster.UpdateResource(ResourceKind.Value, -Cost.Value);

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
}
