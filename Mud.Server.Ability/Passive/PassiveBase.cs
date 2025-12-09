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

    public virtual bool IsTriggered(ICharacter user, ICharacter victim, bool checkImprove, out int diceRoll, out int learnPercentage)
    {
        // 1) get ability learned
        var abilityDefinition = new AbilityDefinition(GetType()); // TODO: should use AbilityManager or directly use a property from passive skill
        var abilityLearned = user.GetAbilityLearnedAndPercentage(abilityDefinition.Name);
        learnPercentage = abilityLearned.percentage;

        // 2) check cooldown
        int cooldownPulseLeft = user.CooldownPulseLeft(abilityDefinition.Name);
        if (cooldownPulseLeft > 0)
        {
            diceRoll = 0;
            return false;
        }

        // 3) check if failed
        diceRoll = RandomManager.Range(1, 100);
        var checkSuccess = CheckSuccess(user, victim, learnPercentage, diceRoll);
        if (!checkSuccess)
            return false;

        // 4) set cooldown
        if (abilityDefinition.CooldownInSeconds.HasValue && abilityDefinition.CooldownInSeconds.Value > 0)
            user.SetCooldown(abilityDefinition.Name, TimeSpan.FromSeconds(abilityDefinition.CooldownInSeconds.Value));

        // 5) check improve true
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
