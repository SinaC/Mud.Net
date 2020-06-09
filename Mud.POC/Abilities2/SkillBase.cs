using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class SkillBase : ISkill, IGameAction
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

        public virtual string Setup(AbilityActionInput abilityActionInput)
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

            // 3) get ability percentage
            var abilityPercentage = User.GetAbilityLearned(AbilityInfo.Name);
            Learned = abilityPercentage.percentage;

            // 4) check targets
            string setTargetResult = SetTargets(abilityActionInput);
            if (setTargetResult != null)
                return setTargetResult;

            // 5) check cooldown
            int cooldownPulseLeft = User.CooldownPulseLeft(AbilityInfo.Name);
            if (cooldownPulseLeft > 0)
                return $"{AbilityInfo.Name} is in cooldown for {StringHelpers.FormatDelay(cooldownPulseLeft / Pulse.PulsePerSeconds)}.";

            return null;
        }

        public virtual void Execute()
        {
            IPlayableCharacter pcUser = User as IPlayableCharacter;

            // 1) invoke skill
            var result = Invoke();

            // 9) GCD
            if (AbilityInfo.PulseWaitTime.HasValue)
                pcUser?.ImpersonatedBy?.SetGlobalCooldown(AbilityInfo.PulseWaitTime.Value);

            // 10) set cooldown
            if (AbilityInfo.Cooldown.HasValue && AbilityInfo.Cooldown.Value > 0)
                User.SetCooldown(AbilityInfo.Name, AbilityInfo.Cooldown.Value);

            // 11) check improve true
            if (pcUser != null)
            {
                var abilityPercentage = User.GetAbilityLearned(AbilityInfo.Name);
                pcUser?.CheckAbilityImprove(abilityPercentage.abilityLearned, result, AbilityInfo.LearnDifficultyMultiplier);
            }
        }

        #endregion

        #region IGameAction

        public string Guards(ActionInput actionInput)
        {
            if (actionInput.Actor == null)
                return "Cannot use a skill without an actor.";
            // check if actor is Character
            var user = actionInput.Actor as ICharacter;
            if (user == null)
                return "Only character are allowed to use skills.";
            var abilityInfo = new AbilityInfo(GetType());
            var abilityActionInput = new AbilityActionInput(abilityInfo, user, user.Level, actionInput.RawParameters, actionInput.Parameters);
            if (DependencyContainer.Current.GetRegistration(AbilityInfo.AbilityExecutionType, false) == null)
                return "Ability not found in DependencyContainer";
            string setupResult = Setup(abilityActionInput);
            return setupResult;
        }

        public void Execute(ActionInput actionInput)
        {
            Execute();
        }

        #endregion

        protected abstract string SetTargets(AbilityActionInput abilityActionInput);
        protected abstract bool Invoke(); // return true if skill has been used with success, false if failed
    }
}
