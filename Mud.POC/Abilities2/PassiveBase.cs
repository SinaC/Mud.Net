using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class PassiveBase : IPassive
    {
        protected IRandomManager RandomManager { get; }

        public PassiveBase(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public virtual bool Use(ICharacter user, ICharacter victim, out int diceRoll)
        {
            diceRoll = 0;

            IPlayableCharacter pcUser = user as IPlayableCharacter;

            // 1) get ability info
            var abilityInfo = new AbilityInfo(GetType());
            var abilityLearned = user.GetAbilityLearned(abilityInfo.Name);

            // 2) check cooldown
            int cooldownPulseLeft = user.CooldownPulseLeft(abilityInfo.Name);
            if (cooldownPulseLeft > 0)
                return false;

            // 3) check if failed
            diceRoll = RandomManager.Range(0, 100);
            var checkSuccess = CheckSuccess(user, victim, abilityLearned.percentage, diceRoll);
            if (!checkSuccess)
                return false;

            // 4) set cooldown
            if (abilityInfo.Cooldown.HasValue && abilityInfo.Cooldown.Value > 0)
                user.SetCooldown(abilityInfo.Name, abilityInfo.Cooldown.Value);

            // 5) check improve true
            pcUser?.CheckAbilityImprove(abilityLearned.abilityLearned, true, abilityInfo.LearnDifficultyMultiplier);

            return true;
        }

        protected virtual bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll) => diceRoll < learnPercentage;
    }
}
