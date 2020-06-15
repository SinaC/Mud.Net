using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;

namespace Mud.Server.Ability.Passive
{
    public abstract class PassiveBase : IPassive
    {
        protected IRandomManager RandomManager { get; }

        public PassiveBase(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public virtual bool IsTriggered(ICharacter user, ICharacter victim, bool checkImprove, out int diceRoll, out int learnPercentage)
        {
            IPlayableCharacter pcUser = user as IPlayableCharacter;

            // 1) get ability info
            var abilityInfo = new AbilityInfo(GetType());
            var abilityLearned = user.GetAbilityLearnedInfo(abilityInfo.Name);
            learnPercentage = abilityLearned.percentage;

            // 2) check cooldown
            int cooldownPulseLeft = user.CooldownPulseLeft(abilityInfo.Name);
            if (cooldownPulseLeft > 0)
            {
                diceRoll = 0;
                return false;
            }

            // 3) check if failed
            diceRoll = RandomManager.Range(0, 100);
            var checkSuccess = CheckSuccess(user, victim, learnPercentage, diceRoll);
            if (!checkSuccess)
                return false;

            // 4) set cooldown
            if (abilityInfo.CooldownInSeconds.HasValue && abilityInfo.CooldownInSeconds.Value > 0)
                user.SetCooldown(abilityInfo.Name, TimeSpan.FromSeconds(abilityInfo.CooldownInSeconds.Value));

            // 5) check improve true
            if (checkImprove)
                pcUser?.CheckAbilityImprove(abilityInfo.Name, true, abilityInfo.LearnDifficultyMultiplier);

            return true;
        }

        protected virtual bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll) => diceRoll < learnPercentage;
    }
}
