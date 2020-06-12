using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;

namespace Mud.Server.Ability.Skill
{
    public abstract class SkillBase : ISkill, IGameAction
    {
        protected IRandomManager RandomManager { get; }

        protected bool IsSetupExecuted { get; private set; }
        protected IAbilityInfo AbilityInfo { get; private set; }
        protected ICharacter User { get; private set; }
        protected int Learned { get; private set; }

        protected SkillBase(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        #region ISkill

        public virtual string Setup(ISkillActionInput skillActionInput)
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
            var abilityPercentage = User.GetAbilityLearnedInfo(AbilityInfo.Name);
            Learned = abilityPercentage.percentage;

            // 4) check targets
            string setTargetResult = SetTargets(skillActionInput);
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
                pcUser?.CheckAbilityImprove(AbilityInfo.Name, result, AbilityInfo.LearnDifficultyMultiplier);
        }

        #endregion

        #region IGameAction

        public string Guards(IActionInput actionInput)
        {
            if (actionInput.Actor == null)
                return "Cannot use a skill without an actor.";
            // check if actor is Character
            var user = actionInput.Actor as ICharacter;
            if (user == null)
                return "Only character are allowed to use skills.";
            var abilityInfo = new AbilityInfo(GetType());
            var skillActionInput = new SkillActionInput(actionInput, abilityInfo, user);
            string setupResult = Setup(skillActionInput);
            return setupResult;
        }

        public void Execute(IActionInput actionInput)
        {
            Execute();
        }

        #endregion

        protected abstract string SetTargets(ISkillActionInput skillActionInput);
        protected abstract bool Invoke(); // return true if skill has been used with success, false if failed
    }
}
