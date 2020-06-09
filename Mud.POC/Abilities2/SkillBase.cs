using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class SkillBase : ISkill
    {
        protected IRandomManager RandomManager { get; }

        protected AbilityInfo AbilityInfo { get; private set; }
        protected ICharacter User { get; private set; }
        protected int Learned { get; private set; }

        protected SkillBase(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        #region ISkill

        public string Setup(AbilityActionInput abilityActionInput)
        {
            // 1) check context
            AbilityInfo = abilityActionInput.AbilityInfo;
            if (AbilityInfo == null)
                return "Internal error: AbilityInfo is null.";
            if (AbilityInfo.AbilityExecutionType != GetType())
                return $"Internal error: AbilityInfo is not of the right type: {AbilityInfo.GetType().Name} instead of {GetType().Name}.";
            // 2) check actor
            User = abilityActionInput.Actor as ICharacter;
            if (User == null)
                return "Skill must be used by a character.";
            if (User.Room == null)
                return "You are nowhere...";

            // 3) check targets
            string setTargetResult = SetTargets(abilityActionInput);
            if (setTargetResult != null)
                return setTargetResult;

            // 4) check cooldown
            int cooldownPulseLeft = User.CooldownPulseLeft(AbilityInfo.Name);
            if (cooldownPulseLeft > 0)
                return $"{AbilityInfo.Name} is in cooldown for {StringHelpers.FormatDelay(cooldownPulseLeft / Pulse.PulsePerSeconds)}.";

            return null;
        }

        public void Execute()
        {
            IPlayableCharacter pcUser = User as IPlayableCharacter;

            // 1) invoke skill
            var abilityPercentage = User.GetAbilityLearned(AbilityInfo.Name);
            Learned = abilityPercentage.percentage;
            var result = Invoke();
            if (result == InvokeResults.Other)
                return;

            // 9) GCD
            if (AbilityInfo.PulseWaitTime.HasValue)
                pcUser?.ImpersonatedBy?.SetGlobalCooldown(AbilityInfo.PulseWaitTime.Value);

            // 10) set cooldown
            if (AbilityInfo.Cooldown.HasValue && AbilityInfo.Cooldown.Value > 0)
                User.SetCooldown(AbilityInfo.Name, AbilityInfo.Cooldown.Value);

            // 11) check improve true
            pcUser?.CheckAbilityImprove(abilityPercentage.abilityLearned, result == InvokeResults.Ok, AbilityInfo.LearnDifficultyMultiplier);
        }

        #endregion

        protected abstract string SetTargets(AbilityActionInput abilityActionInput);
        protected abstract InvokeResults Invoke(); // return true if skill has been used with success

        protected enum InvokeResults
        {
            Ok, // used successfully
            Failed, // failed on ability percentage check
            Other, // was not used
        }
    }
}
