using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills
{
    [Command("trip", "Abilities", "Skills", "Combat")]
    [Skill(SkillName, AbilityEffects.Damage, PulseWaitTime = 24)]
    public class Trip : OffensiveSkillBase
    {
        public const string SkillName = "Trip";

        public Trip(IRandomManager randomManager)
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
                || (npcUser != null && !npcUser.OffensiveFlags.HasFlag(OffensiveFlags.Trip)))
                return "Tripping?  What's that?";

            if (Victim == User)
            {
                User.Act(ActOptions.ToRoom, "{0:N} trips over {0:s} own feet!", User);
                return "You fall flat on your face!";
            }

            if (Victim.IsSafe(User))
                return "Not on that victim.";

            // TODO: check kill stealing

            if (User.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcUser?.Master == Victim)
                return User.ActPhrase("But {0:N} is your friend!", Victim);

            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Flying))
                return User.ActPhrase("{0:s} feet aren't on the ground.", Victim);

            if (Victim.Position < Positions.Fighting)
                return User.ActPhrase("{0:N} is already down..", Victim);

            return null;
        }

        protected override bool Invoke()
        {
            int chance = Learned;
            // modifiers
            if (User.Size < Victim.Size)
                chance -= (Victim.Size - User.Size) * 10; // bigger = harder to trip
            // dexterity
            chance += User[CharacterAttributes.Dexterity];
            chance -= (3 * Victim[CharacterAttributes.Dexterity]) / 2;
            // speed
            if ((User as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || User.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((Victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || Victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 20;
            // level
            chance += (User.Level - Victim.Level) * 2;

            // now the attack
            if (RandomManager.Chance(chance))
            {
                Victim.Act(ActOptions.ToCharacter, "{0:N} trips you and you go down!", User);
                User.Act(ActOptions.ToCharacter, "You trip {0} and {0} goes down!", Victim);
                User.ActToNotVictim(Victim, "{0} trips {1}, sending {1:m} to the ground.", User, Victim);
                //DAZE_STATE(Victim, 2 * PULSE_VIOLENCE);
                Victim.ChangePosition(Positions.Resting);
                // TODO: check_killer(ch, Victim)
                int damage = RandomManager.Range(2, 2 + 2 * (int)User.Size);
                Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "trip", true);
                return true;
            }
            else
            {
                Victim.AbilityDamage(User, 0, SchoolTypes.Bash, "trip", true);
                // TODO check_killer(ch,Victim);
                return false;
            }
        }
    }
}
