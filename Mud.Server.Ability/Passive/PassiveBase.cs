using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Passive;

public abstract class PassiveBase : IPassive
{
    protected ILogger<PassiveBase> Logger { get; }
    protected IRandomManager RandomManager { get; }

    protected PassiveBase(ILogger<PassiveBase> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    protected abstract string Name { get; }

    public virtual bool IsTriggered(ICharacter user, ICharacter victim, bool checkImprove, out int diceRoll, out int learnPercentage)
    {
        // 1) get ability learned
        var (percentage, abilityLearned) = user.GetAbilityLearnedAndPercentage(Name);

        // 2) cannot trigger if not learned
        if (abilityLearned == null)
        {
            diceRoll = 0;
            learnPercentage = 0;
            return false;
        }
        learnPercentage = percentage;
        var abilityDefinition = abilityLearned.AbilityUsage.AbilityDefinition;

        // 3) check guards
        if (abilityDefinition.Guards.Length > 0)
        {
            foreach (var guard in abilityDefinition.Guards)
            {
                var guardResult = guard.Guards(user);
                if (guardResult != null)
                {
                    diceRoll = 0;
                    return false;
                }
            }
        }

        // 3) check cooldown
        int cooldownPulseLeft = user.CooldownPulseLeft(abilityDefinition.Name);
        if (cooldownPulseLeft > 0)
        {
            diceRoll = 0;
            return false;
        }

        // 4) check if failed
        diceRoll = RandomManager.Range(1, 100);
        var checkSuccess = CheckSuccess(user, victim, learnPercentage, diceRoll);
        if (!checkSuccess)
            return false;

        // 5) set cooldown
        if (abilityDefinition.CooldownInSeconds.HasValue && abilityDefinition.CooldownInSeconds.Value > 0)
            user.SetCooldown(abilityDefinition.Name, TimeSpan.FromSeconds(abilityDefinition.CooldownInSeconds.Value));

        // 6) check improve true
        if (checkImprove)
        {
            var pcUser = user as IPlayableCharacter;
            pcUser?.CheckAbilityImprove(abilityDefinition.Name, true, abilityDefinition.LearnDifficultyMultiplier);
        }

        return true;
    }

    protected virtual bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
        => diceRoll < learnPercentage;
}
