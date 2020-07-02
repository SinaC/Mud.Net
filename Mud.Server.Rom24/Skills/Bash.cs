using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using Mud.Server.Rom24.Passives;

namespace Mud.Server.Rom24.Skills
{
    [CharacterCommand("bash", "Ability", "Skill", "Combat")]
    [Syntax("[cmd] <victim>")]
    [Skill(SkillName, AbilityEffects.Damage, PulseWaitTime = 20)]
    public class Bash : OffensiveSkillBase
    {
        public const string SkillName = "Bash";

        public Bash(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            INonPlayableCharacter npcUser = User as INonPlayableCharacter;
            if (Learned == 0
                || (npcUser != null && !npcUser.OffensiveFlags.IsSet("Bash")))
                return "Bashing? What's that?";

            if (Victim == User)
                return "You try to bash your brains out, but fail.";

            string safeResult = Victim.IsSafe(User);
            if (safeResult != null)
                return safeResult;

            if (Victim.Position < Positions.Fighting)
                return User.ActPhrase("You'll have to let {0:m} get back up first.", Victim);

            // TODO: check kill stealing

            if (User.CharacterFlags.IsSet("Charm") && npcUser?.Master == Victim)
                return User.ActPhrase("But {0:N} is your friend!", Victim);

            return null;
        }

        protected override bool Invoke()
        {
            int chance = Learned;

            // modifiers
            // size and weight
            chance += User.CarryWeight / 250;
            chance -= Victim.CarryWeight / 250;
            if (User.Size < Victim.Size)
                chance -= (Victim.Size - User.Size) * 15; // big drawback to bash someone bigger
            else
                chance += (User.Size - Victim.Size) * 10; // big advantage to bash someone smaller
            // stats
            chance += User[CharacterAttributes.Strength];
            chance -= (4 * Victim[CharacterAttributes.Dexterity]) / 3;
            chance -= Victim[CharacterAttributes.ArmorBash] / 25;
            // speed
            if ((User as INonPlayableCharacter)?.OffensiveFlags.IsSet("Fast") == true || User.CharacterFlags.IsSet("Haste"))
                chance += 10;
            if ((Victim as INonPlayableCharacter)?.OffensiveFlags.IsSet("Fast") == true || Victim.CharacterFlags.IsSet("Haste"))
                chance -= 30;
            // level
            chance += User.Level - Victim.Level;

            // dodge?
            var VictimDodgeInfo = Victim.GetAbilityLearnedInfo(Dodge.PassiveName);
            if (chance < VictimDodgeInfo.percentage)
                chance -= 3 * (VictimDodgeInfo.percentage - chance);

            // now the attack
            if (RandomManager.Chance(chance))
            {
                User.Act(ActOptions.ToCharacter, "You slam into {0}, and send {0:m} flying!", Victim);
                User.Act(ActOptions.ToRoom, "{0:N} sends {1} sprawling with a powerful bash.", User, Victim);
                // TODO: Victim daze
                Victim.ChangePosition(Positions.Resting);
                int damage = RandomManager.Range(2, 2 + 2 * (int)User.Size + chance / 20);
                Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "bash", false);
                // TODO: check_killer(ch,Victim);
                return true;
            }
            Victim.AbilityDamage(User, 0, SchoolTypes.Bash, "bash", false); // starts a fight
            User.Send("You fall flat on your face!");
            User.Act(ActOptions.ToRoom, "{0:N} fall{0:v} flat on {0:s} face!", User);
            Victim.Act(ActOptions.ToCharacter, "You evade {0:p} bash, causing {0:m} to fall flat on {0:s} face.", User);
            User.ChangePosition(Positions.Resting);
            // TODO: check_killer(ch,Victim);
            return false;
        }
    }
}
