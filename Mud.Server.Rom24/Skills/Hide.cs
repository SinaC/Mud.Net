using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Passives;
using Mud.Server.Rom24.Spells;
using System;

namespace Mud.Server.Rom24.Skills
{
    [Command("hide", "Abilities", "Skills")]
    [Skill(SkillName, AbilityEffects.Buff, LearnDifficultyMultiplier = 3)]
    public class Hide : NoTargetSkillBase
    {
        public const string SkillName = "Hide";

        public Hide(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool Invoke()
        {
            User.Send("You attempt to hide.");

            if (User.CharacterFlags.HasFlag(CharacterFlags.Hide))
                User.RemoveBaseCharacterFlags(CharacterFlags.Hide);

            bool success = false;
            if (RandomManager.Chance(Learned))
            {
                User.AddBaseCharacterFlags(CharacterFlags.Hide);
                success = true;
            }

            User.Recompute();
            return success;
        }
    }
}
