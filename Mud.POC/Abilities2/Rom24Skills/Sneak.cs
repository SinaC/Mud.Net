using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;
using System;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("sneak", "Abilities", "Skills")]
    [Skill(SkillName, AbilityEffects.Buff, LearnDifficultyMultiplier = 3)]
    public class Sneak : NoTargetSkillBase
    {
        public const string SkillName = "Sneak";

        private IAuraManager AuraManager { get; }

        public Sneak(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        public override string Setup(SkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            User.Send("You attempt to move silently.");
            User.RemoveAuras(x => x.AbilityName == SkillName, true);

            if (User.CharacterFlags.HasFlag(CharacterFlags.Sneak))
                return string.Empty;

            return null;
        }

        protected override bool Invoke()
        {
            if (RandomManager.Chance(Learned))
            {
                AuraManager.AddAura(User, SkillName, User, User.Level, TimeSpan.FromMinutes(User.Level), AuraFlags.None, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Sneak, Operator = AffectOperators.Or });
                return true;
            }

            return false;
        }
    }
}
