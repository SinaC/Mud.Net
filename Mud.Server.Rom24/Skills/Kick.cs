﻿using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills
{
    [CharacterCommand("kick", "Ability", "Skill", "Combat")]
    [Syntax("[cmd] <victim>")]
    [Skill(SkillName, AbilityEffects.Damage)]
    public class Kick : FightingSkillBase
    {
        public const string SkillName = "Kick";

        public Kick(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (Learned == 0 || (User is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.IsSet("Kick")))
                return "You better leave the martial arts to fighters.";

            return null;
        }

        protected override bool Invoke()
        {
            if (RandomManager.Chance(Learned))
            {
                int damage = RandomManager.Range(1, User.Level);
                Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "kick", true);
                //check_killer(ch,victim);
                return true;
            }
            // No need to start a fight because this ability can only used in combat
            return false;
        }
    }
}
