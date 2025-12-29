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
    protected IAbilityDefinition AbilityDefinition { get; }

    protected bool IsSetupExecuted { get; private set; }
    protected ICharacter User { get; private set; } = default!;
    protected int Learned { get; private set; }
    protected ResourceCostToPay[] ResourceCostsToPay { get; private set; } = default!;

    protected SkillBase(ILogger<SkillBase> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
        AbilityDefinition = new AbilityDefinition(GetType());
    }

    #region ISkill

    public virtual string? Setup(ISkillActionInput skillActionInput)
    {
        IsSetupExecuted = true;

        // 1) check actor
        User = skillActionInput.User;
        if (User == null)
            return "Skill must be used by a character.";
        if (User.Room == null)
            return "You are nowhere...";

        // 2) check shape
        if (AbilityDefinition.AllowedShapes?.Length > 0 && !AbilityDefinition.AllowedShapes.Contains(User.Shape))
        {
            if (AbilityDefinition.AllowedShapes.Length > 1)
                return $"You are not in {string.Join(" or ", AbilityDefinition.AllowedShapes.Select(x => x.ToString()))} shape";
            return $"You are not in {AbilityDefinition.AllowedShapes.Single()} shape";
        }

        // 4) get ability percentage
        var (percentage, abilityLearned) = User.GetAbilityLearnedAndPercentage(AbilityDefinition.Name);
        Learned = percentage;

        // 5) if skill must be learned, check if learned
        if (MustBeLearned && abilityLearned == null)
            return "You don't know any skills of that name.";

        // 5) check costs
        if (abilityLearned != null && abilityLearned.HasCost)
        {
            var resourceCostToPays = new List<ResourceCostToPay>();
            foreach (var abilityResourceCost in abilityLearned.AbilityUsage.ResourceCosts)
            {
                // TODO: check each costs
                var resourceKind = abilityResourceCost.ResourceKind;
                if (!User.CurrentResourceKinds.Contains(resourceKind)) // TODO: not sure about this test
                    return $"You can't use {resourceKind} as resource for the moment.";
                int resourceLeft = User[resourceKind];
                int cost;
                bool isAll = false;
                switch (abilityResourceCost.CostAmountOperator)
                {
                    case CostAmountOperators.Fixed:
                        cost = abilityResourceCost.CostAmount;
                        break;
                    case CostAmountOperators.Percentage:
                        cost = User.MaxResource(resourceKind) * abilityResourceCost.CostAmount / 100;
                        break;
                    case CostAmountOperators.All:
                        cost = User[resourceKind];
                        isAll = true;
                        break;
                    case CostAmountOperators.AllWithMin:
                        cost = Math.Max(abilityResourceCost.CostAmount, User[resourceKind]);
                        isAll = true;
                        break;
                    default:
                        Logger.LogError("Unexpected CostAmountOperator {costAmountOperator} for ability {abilityName}.", abilityResourceCost.CostAmountOperator, AbilityDefinition.Name);
                        cost = 100;
                        break;
                }
                bool enoughResource = cost <= resourceLeft;
                if (!enoughResource)
                    return $"You don't have enough {resourceKind}.";
                var resourceCostToPay = new ResourceCostToPay(resourceKind, cost, isAll);
                resourceCostToPays.Add(resourceCostToPay);
            }
            ResourceCostsToPay = resourceCostToPays.ToArray();
        }
        else
        {
            ResourceCostsToPay = [];
        }

        // 6) check targets
        var setTargetResult = SetTargets(skillActionInput);
        if (setTargetResult != null)
            return setTargetResult;

        // 7) check cooldown
        int cooldownPulseLeft = User.CooldownPulseLeft(AbilityDefinition.Name);
        if (cooldownPulseLeft > 0)
            return $"{AbilityDefinition.Name} is in cooldown for {(cooldownPulseLeft / Pulse.PulsePerSeconds).FormatDelay()}.";

        return null;
    }

    public virtual void Execute()
    {
        if (!IsSetupExecuted)
            throw new Exception("Cannot execute skill without calling setup first.");


        // 1) invoke skill
        var result = Invoke();

        // 2) pay costs
        foreach(var resourceCostToPay in ResourceCostsToPay.Where(x => x.CostAmount > 0))
        {
            if (result || resourceCostToPay.IsAll)
                User.UpdateResource(resourceCostToPay.ResourceKind, -resourceCostToPay.CostAmount);
            else
                User.UpdateResource(resourceCostToPay.ResourceKind, -resourceCostToPay.CostAmount/2); // half cost if failed and not 'all'
        }

        // 3) GCD
        if (AbilityDefinition.PulseWaitTime.HasValue)
            User.SetGlobalCooldown(AbilityDefinition.PulseWaitTime.Value);

        // 4) set cooldown
        if (AbilityDefinition.CooldownInSeconds.HasValue && AbilityDefinition.CooldownInSeconds.Value > 0)
            User.SetCooldown(AbilityDefinition.Name, TimeSpan.FromSeconds(AbilityDefinition.CooldownInSeconds.Value));

        // 5) check improve true
        var pcUser = User as IPlayableCharacter;
        pcUser?.CheckAbilityImprove(AbilityDefinition.Name, result, AbilityDefinition.LearnDifficultyMultiplier);
    }

    #endregion

    #region IGameAction

    public sealed override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var skillActionInput = new SkillActionInput(actionInput, AbilityDefinition, Actor);
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

    protected record ResourceCostToPay(ResourceKinds ResourceKind, int CostAmount, bool IsAll);
}
