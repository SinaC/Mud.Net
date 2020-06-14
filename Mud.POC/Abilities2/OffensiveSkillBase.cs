using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Random;

namespace Mud.POC.Abilities2
{
    public abstract class OffensiveSkillBase : SkillBase
    {
        protected ICharacter Victim { get; set; }

        protected OffensiveSkillBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(ISkillActionInput skillActionInput)
        {
            if (skillActionInput.Parameters.Length < 1)
            {
                Victim = User.Fighting;
                if (Victim == null)
                    return "Use the skill on whom?";
            }
            else
                Victim = FindHelpers.FindByName(User.Room.People, skillActionInput.Parameters[0]);
            if (Victim == null)
                return "They aren't here.";
            if (User is IPlayableCharacter)
            {
                if (User != Victim && Victim.IsSafe(User))
                    return "Not on that victim.";
                // TODO: check_killer
            }
            if (User is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == Victim)
                return "You can't do that on your own follower.";
            // victim found
            return null;
        }
    }
}
