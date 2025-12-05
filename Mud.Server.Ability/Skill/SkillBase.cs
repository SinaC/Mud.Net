using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Random;

namespace Mud.Server.Ability.Skill;

public abstract class SkillBase : CharacterGameAction, ISkill
{
    protected ILogger<SkillBase> Logger { get; }
    protected IRandomManager RandomManager { get; }

    protected bool IsSetupExecuted { get; private set; }
    protected IAbilityInfo AbilityInfo { get; private set; } = default!;
    protected ICharacter User { get; private set; } = default!;
    protected int Learned { get; private set; }
    protected int? Cost { get; private set; }
    protected ResourceKinds? ResourceKind { get; private set; }

    protected SkillBase(ILogger<SkillBase> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    #region ISkill

    public virtual string? Setup(ISkillActionInput skillActionInput)
    {
        IsSetupExecuted = true;

        // 1) check context
        AbilityInfo = skillActionInput.AbilityInfo;
        if (AbilityInfo == null)
            return "Internal error: AbilityInfo is null.";
        if (AbilityInfo.AbilityExecutionType != GetType())
            return $"Internal error: AbilityInfo is not of the right type: {AbilityInfo.GetType().Name} instead of {GetType().Name}.";

        // 2) check actor
        User = skillActionInput.User;
        if (User == null)
            return "Skill must be used by a character.";
        if (User.Room == null)
            return "You are nowhere...";

        // 3) get ability percentage
        var (percentage, abilityLearned) = User.GetAbilityLearnedInfo(AbilityInfo.Name);
        Learned = percentage;

        // 4) check resource cost
        if (abilityLearned != null && abilityLearned.ResourceKind.HasValue && abilityLearned.CostAmount > 0 && abilityLearned.CostAmountOperator != CostAmountOperators.None)
        {
            var resourceKind = abilityLearned.ResourceKind.Value;
            if (!User.CurrentResourceKinds.Contains(resourceKind)) // TODO: not sure about this test
                return $"You can't use {resourceKind} as resource for the moment.";
            int resourceLeft = User[resourceKind];
            int cost;
            switch (abilityLearned.CostAmountOperator)
            {
                case CostAmountOperators.Fixed:
                    cost = abilityLearned.CostAmount;
                    break;
                case CostAmountOperators.Percentage:
                    cost = User.MaxResource(resourceKind) * abilityLearned.CostAmount / 100;
                    break;
                default:
                    Logger.LogError("Unexpected CostAmountOperator {costAmountOperator} for ability {abilityName}.", abilityLearned.CostAmountOperator, AbilityInfo.Name);
                    cost = 100;
                    break;
            }
            bool enoughResource = cost <= resourceLeft;
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

        // 5) check targets
        var setTargetResult = SetTargets(skillActionInput);
        if (setTargetResult != null)
            return setTargetResult;

        // 5) check cooldown
        int cooldownPulseLeft = User.CooldownPulseLeft(AbilityInfo.Name);
        if (cooldownPulseLeft > 0)
            return $"{AbilityInfo.Name} is in cooldown for {(cooldownPulseLeft / Pulse.PulsePerSeconds).FormatDelay()}.";

        return null;
    }

    public virtual void Execute()
    {
        if (!IsSetupExecuted)
            throw new Exception("Cannot execute skill without calling setup first.");

        var pcUser = User as IPlayableCharacter;

        // 1) invoke skill
        var result = Invoke();

        // 2) pay resource
        if (Cost.HasValue && Cost.Value >= 1 && ResourceKind.HasValue)
        {
            if (result)
                User.UpdateResource(ResourceKind.Value, -Cost.Value);
            else
                User.UpdateResource(ResourceKind.Value, -Cost.Value / 2); // half cost if failed
        }

        // 3) GCD
        if (AbilityInfo.PulseWaitTime.HasValue)
            pcUser?.ImpersonatedBy?.SetGlobalCooldown(AbilityInfo.PulseWaitTime.Value);

        // 4) set cooldown
        if (AbilityInfo.CooldownInSeconds.HasValue && AbilityInfo.CooldownInSeconds.Value > 0)
            User.SetCooldown(AbilityInfo.Name, TimeSpan.FromSeconds(AbilityInfo.CooldownInSeconds.Value));

        // 5) check improve true
        pcUser?.CheckAbilityImprove(AbilityInfo.Name, result, AbilityInfo.LearnDifficultyMultiplier);
    }

    #endregion

    #region IGameAction

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var abilityInfo = new AbilityInfo(GetType());
        var skillActionInput = new SkillActionInput(actionInput, abilityInfo, Actor);
        var setupResult = Setup(skillActionInput);
        return setupResult;
    }

    public override void Execute(IActionInput actionInput)
    {
        Execute();
    }

    #endregion

    protected abstract string? SetTargets(ISkillActionInput skillActionInput);
    protected abstract bool Invoke(); // return true if skill has been used with success, false if failed
}
