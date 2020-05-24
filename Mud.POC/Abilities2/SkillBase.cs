using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class SkillBase : ISkill
    {

        public abstract int Id { get; }

        public abstract string Name { get; }

        public virtual int PulseWaitTime => 12;

        public virtual int Cooldown => 0;

        public virtual int LearnDifficultyMultiplier => 1;

        public virtual AbilityFlags Flags => AbilityFlags.None;

        public abstract AbilityEffects Effects { get; }

        public virtual UseResults Use(ICharacter user, IAbility ability, string rawParameters, params CommandParameter[] parameters)
        {
            // 1) get targets if any
            GetTargets(rawParameters, parameters);

            // 2) check cooldown
            int cooldownPulseLeft = user.CooldownPulseLeft(ability);
            if (cooldownPulseLeft > 0)
            {
                user.Send("{0} is in cooldown for {1}.", ability.Name, StringHelpers.FormatDelay(cooldownPulseLeft / Pulse.PulsePerSeconds));
                return UseResults.InCooldown;
            }

            // 3) invoke skill

            // 4) GCD

            // 5) set cooldown
            if (ability.Cooldown > 0)
                user.SetCooldown(ability);

            // 6) check improve true

            // 7) if aggressive: multi hit if still in same room

            return UseResults.Ok;
        }

        protected abstract void GetTargets(string rawParameters, params CommandParameter[] parameters);
    }
}
