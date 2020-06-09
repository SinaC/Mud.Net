using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Skill(SkillName, AbilityEffects.Buff, LearnDifficultyMultiplier = 3)]
    public class Sneak : SkillBase
    {
        public const string SkillName = "Sneak";

        private IAuraManager AuraManager { get; }

        public Sneak(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput) => null;

        protected override InvokeResults Invoke()
        {
            User.Send("You attempt to move silently.");
            User.RemoveAuras(x => x.AbilityName == SkillName, true);

            if (User.CharacterFlags.HasFlag(CharacterFlags.Sneak))
                return InvokeResults.Other;

            if (RandomManager.Chance(Learned))
            {
                AuraManager.AddAura(User, SkillName, User, User.Level, TimeSpan.FromMinutes(User.Level), AuraFlags.None, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Sneak, Operator = AffectOperators.Or });
                return InvokeResults.Ok;
            }

            return InvokeResults.Failed;
        }
    }
}
