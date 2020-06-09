using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("kick", "Abilities", "Skills", "Combat")]
    [Skill(SkillName, AbilityEffects.Damage)]
    public class Kick : FightingSkillBase
    {
        public const string SkillName = "Kick";

        public Kick(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        public override string Setup(AbilityActionInput abilityActionInput)
        {
            string baseSetup = base.Setup(abilityActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (Learned == 0 || (User is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Kick)))
                return "You better leave the martial arts to fighters.";

            return null;
        }

        protected override bool Invoke()
        {
            if (RandomManager.Chance(Learned))
            {
                int damage = RandomManager.Range(1, User.Level);
                Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "Kick", true);
                //check_killer(ch,victim);
                return true;
            }
            else
            {
                Victim.AbilityDamage(User, 0, SchoolTypes.Bash, "Kick", true);
                //check_killer(ch,victim);
                return false;
            }
        }
    }
}
