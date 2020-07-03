﻿using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Table;
using Mud.Server.Random;

namespace Mud.Server.AdditionalAbilities
{
    [CharacterCommand("crush", "Ability", "Skill", "Combat")]
    [Skill(SkillName, AbilityEffects.Damage, LearnDifficultyMultiplier = 3, PulseWaitTime = 12)]
    public class Crush : FightingSkillBase
    {
        private const string SkillName = "Crush";

        private ITableValues TableValues { get; }

        public Crush(IRandomManager randomManager, ITableValues tableValues)
            : base(randomManager)
        {
            TableValues = tableValues;
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            INonPlayableCharacter npcUser = User as INonPlayableCharacter;
            if (Learned == 0
                || (npcUser != null && !npcUser.OffensiveFlags.IsSet("Crush")))
                return "Crushing, how do you do that again?";

            return null;
        }

        protected override bool Invoke()
        {
            int chance = Learned;
            chance -= chance / 4;
            chance += (User.Level - Victim.Level) * 2;
            chance += User[BasicAttributes.Strength];
            chance -= Victim[BasicAttributes.Dexterity] / 3;
            chance -= Victim[BasicAttributes.Strength] / 2;

            int sizeMultiplier = User.Size < Victim.Size ? 25 : 10;
            chance += (User.Size - Victim.Size) * sizeMultiplier;

            if (RandomManager.Chance(chance))
            {
                Victim.Act(ActOptions.ToAll, "{0:N} grab{0:v} {1:n} and slam{0:v} {1:m} to the ground with bone crushing force!", User, Victim);
                int damage;
                if (User.Level < 20)
                    damage = 20;
                else if (User.Level < 25)
                    damage = 30;
                else if (User.Level < 30)
                    damage = 40;
                else if (User.Level < 35)
                    damage = 50;
                else if (User.Level < 40)
                    damage = 60;
                else if (User.Level < 52)
                    damage = 70;
                else
                    damage = 70;
                damage += TableValues.DamBonus(User);
                Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "crush", true);
                Victim.ChangePosition(Positions.Resting);
                //WAIT_STATE(victim,PULSE_VIOLENCE); TODO ?
                //check_killer(ch,victim);
                return true;
            }

            User.Act(ActOptions.ToCharacter, "Your crush attempt misses {0:N}.", Victim);
            User.Act(ActOptions.ToRoom, "{0:N} lashes out wildly with {0:s} arms but misses.", User);
            //check_killer(ch,victim);

            // No need to start a fight because this ability can only used in combat
            return false;
        }
    }
}
