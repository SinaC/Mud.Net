using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Passives;
using Mud.Server.Rom24.Spells;
using System;
using System.Linq;
using System.Text;

namespace Mud.Server.Rom24.Skills
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

        public override string Setup(ISkillActionInput skillActionInput)
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
