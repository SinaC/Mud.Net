using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.AdditionalAbilities
{
    [CharacterCommand("tail", "Ability", "Skills", "Combat")]
    [Skill(SkillName, AbilityEffects.Damage, LearnDifficultyMultiplier = 1, PulseWaitTime = 18)]
    public class Tail : FightingSkillBase
    {
        private const string SkillName = "Tail";

        public Tail(IRandomManager randomManager)
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
                || (npcUser != null && !npcUser.OffensiveFlags.IsSet("Tail")))
                return "You don't have any tail...";

            return null;
        }

        protected override bool Invoke()
        {
            if (RandomManager.Chance(Learned))
            {
                int damage = RandomManager.Range(2*User.Level, 3*User.Level);
                Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "tailsweep", true);
                //DAZE_STATE(victim, 2 * PULSE_VIOLENCE);
                //check_killer(ch,victim);
                return true;
            }
            //check_killer(ch,victim);
            // No need to start a fight because this ability can only used in combat
            return false;
        }
    }
}
