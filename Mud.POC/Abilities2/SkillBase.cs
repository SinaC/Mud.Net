using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class SkillBase : ISkill
    {
        protected IRandomManager RandomManager { get; }
        protected IWiznet Wiznet { get; }

        public SkillBase(IRandomManager randomManager, IWiznet wiznet)
        {
            RandomManager = randomManager;
            Wiznet = wiznet;
        }

        public abstract int Id { get; }

        public abstract string Name { get; }

        public virtual int PulseWaitTime => 12;

        public virtual int Cooldown => 0;

        public virtual int LearnDifficultyMultiplier => 1;

        public virtual AbilityFlags Flags => AbilityFlags.None;

        public abstract AbilityEffects Effects { get; }

        public virtual UseResults Use(ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            IPlayableCharacter pcUser = user as IPlayableCharacter;

            // 1) get targets if any
            IEntity target;
            AbilityTargetResults targetResult = GetTarget(user, out target, rawParameters, parameters);
            if (targetResult != AbilityTargetResults.Ok)
                return MapAbilityTargetResultsToUseResults(targetResult);

            // 2) check cooldown
            int cooldownPulseLeft = user.CooldownPulseLeft(this);
            if (cooldownPulseLeft > 0)
            {
                user.Send("{0} is in cooldown for {1}.", Name, StringHelpers.FormatDelay(cooldownPulseLeft / Pulse.PulsePerSeconds));
                return UseResults.InCooldown;
            }

            // 4) invoke skill
            var abilityLearnInfo = user.GetLearnInfo(this);
            UseResults result = Invoke(abilityLearnInfo.learned, user, target, rawParameters, parameters);

            // 4) GCD
            pcUser?.ImpersonatedBy?.SetGlobalCooldown(PulseWaitTime);

            // 5) set cooldown
            if (Cooldown > 0)
                user.SetCooldown(this);

            // 6) check improve true
            if (result == UseResults.Ok || result == UseResults.Failed)
                pcUser?.CheckAbilityImprove(abilityLearnInfo.ability, result == UseResults.Ok, LearnDifficultyMultiplier);

            return result;
        }

        protected abstract AbilityTargetResults GetTarget(ICharacter user, out IEntity target, string rawParameters, params CommandParameter[] parameters);
        protected abstract UseResults Invoke(int learned, ICharacter source, IEntity target, string rawParameters, params CommandParameter[] parameters);
        protected virtual void PostInvoke(ICharacter user, IEntity target)
        {
            // NOP
        }

        private UseResults MapAbilityTargetResultsToUseResults(AbilityTargetResults result)
        {
            switch (result)
            {
                case AbilityTargetResults.MissingParameter:
                    return UseResults.MissingParameter;
                case AbilityTargetResults.InvalidTarget:
                    return UseResults.InvalidTarget;
                case AbilityTargetResults.TargetNotFound:
                    return UseResults.TargetNotFound;
                case AbilityTargetResults.Error:
                    return UseResults.Error;
                default:
                    Wiznet.Wiznet($"Unexpected AbilityTargetResults {result}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return UseResults.Error;
            }
        }

        #region IEquatable

        public bool Equals(IAbility other)
        {
            return Id == other.Id;
        }

        #endregion
    }
}
