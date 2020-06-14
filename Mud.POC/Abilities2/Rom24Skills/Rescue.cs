using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("rescue", "Abilities", "Skills", "Combat")]
    [Skill(SkillName, AbilityEffects.None)]
    public class Rescue : OffensiveSkillBase
    {
        public const string SkillName = "Rescue";

        public Rescue(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool Invoke()
        {
            if (!RandomManager.Chance(Learned))
            {
                User.Send("You fail the rescue.");
                return false;
            }

            User.Act(ActOptions.ToAll, "{0:N} rescue{0:v} {1:N}.", User, Victim);
            Victim.Fighting.StopFighting(false);
            Victim.StopFighting(false);
            User.StopFighting(false);

            // TODO: check killer
            User.StartFighting(Victim);
            Victim.StartFighting(User);

            return true;
        }

        protected override string SetTargets(ISkillActionInput skillActionInput)
        {
            string baseSetTargets = base.SetTargets(skillActionInput);
            if (baseSetTargets != null)
                return baseSetTargets;

            if (User == Victim)
                return "What about fleeing instead?";

            if (Victim is INonPlayableCharacter && User is IPlayableCharacter)
                return "Doesn't need your help!";

            if (User.Fighting == Victim)
                return "Too late.";

            ICharacter fighting = Victim.Fighting;
            if (fighting == null)
                return "That person is not fighting right now.";

            if (fighting is INonPlayableCharacter && User.IsSameGroupOrPet(Victim))
                return "Kill stealing is not permitted.";

            return null;
        }
    }
}
