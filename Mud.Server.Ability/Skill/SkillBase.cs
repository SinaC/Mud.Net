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
    protected IAbilityDefinition AbilityDefinition { get; private set; } = default!;
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
        var abilityDefinition = skillActionInput.AbilityDefinition;
        if (abilityDefinition == null)
            return "Internal error: AbilityDefinition is null.";
        if (abilityDefinition.AbilityExecutionType != GetType())
            return $"Internal error: AbilityDefinition is not of the right type: {abilityDefinition.GetType().Name} instead of {GetType().Name}.";
        AbilityDefinition = abilityDefinition;

        // 2) check actor
        User = skillActionInput.User;
        if (User == null)
            return "Skill must be used by a character.";
        if (User.Room == null)
            return "You are nowhere...";

        // 3) check shape
        if (abilityDefinition.AllowedShapes != null && !abilityDefinition.AllowedShapes.Contains(User.Shape))
        {
            if (abilityDefinition.AllowedShapes.Length > 1)
                return $"You are not in {string.Join(" or ", abilityDefinition.AllowedShapes.Select(x => x.ToString()))} shape";
            return $"You are not in {abilityDefinition.AllowedShapes.Single().ToString()} shape";
        }

        // 4) get ability percentage
        var (percentage, abilityLearned) = User.GetAbilityLearnedAndPercentage(abilityDefinition.Name);
        Learned = percentage;

        // 5) if skill must be learned, check if learned
        if (MustBeLearned && abilityLearned == null)
            return "You don't know any skills of that name.";

        // 5) check resource cost
        if (abilityLearned != null && abilityLearned.HasCost())
        {
            var resourceKind = abilityLearned.ResourceKind!.Value;
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
                case CostAmountOperators.All:
                    cost = Math.Max(abilityLearned.CostAmount, User[resourceKind]);
                    break;
                default:
                    Logger.LogError("Unexpected CostAmountOperator {costAmountOperator} for ability {abilityName}.", abilityLearned.CostAmountOperator, abilityDefinition.Name);
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

        // 6) check targets
        var setTargetResult = SetTargets(skillActionInput);
        if (setTargetResult != null)
            return setTargetResult;

        // 7) check cooldown
        int cooldownPulseLeft = User.CooldownPulseLeft(abilityDefinition.Name);
        if (cooldownPulseLeft > 0)
            return $"{abilityDefinition.Name} is in cooldown for {(cooldownPulseLeft / Pulse.PulsePerSeconds).FormatDelay()}.";

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
        if (AbilityDefinition.PulseWaitTime.HasValue)
            pcUser?.ImpersonatedBy?.SetGlobalCooldown(AbilityDefinition.PulseWaitTime.Value);

        // 4) set cooldown
        if (AbilityDefinition.CooldownInSeconds.HasValue && AbilityDefinition.CooldownInSeconds.Value > 0)
            User.SetCooldown(AbilityDefinition.Name, TimeSpan.FromSeconds(AbilityDefinition.CooldownInSeconds.Value));

        // 5) check improve true
        pcUser?.CheckAbilityImprove(AbilityDefinition.Name, result, AbilityDefinition.LearnDifficultyMultiplier);
    }

    #endregion

    #region IGameAction

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var abilityDefinition = new AbilityDefinition(GetType());
        var skillActionInput = new SkillActionInput(actionInput, abilityDefinition, Actor);
        var setupResult = Setup(skillActionInput);
        return setupResult;
    }

    public override void Execute(IActionInput actionInput)
    {
        Execute();
    }

    #endregion

    protected virtual bool MustBeLearned => false; // by default, everyone can use every skills and skill is responsible to check what to do if not learned

    protected abstract string? SetTargets(ISkillActionInput skillActionInput);
    protected abstract bool Invoke(); // return true if skill has been used with success, false if failed
}
