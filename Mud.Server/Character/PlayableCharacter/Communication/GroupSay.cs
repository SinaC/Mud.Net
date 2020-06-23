using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.PlayableCharacter.Communication
{
    [PlayableCharacterCommand("groupsay", "Group", "Communication", Priority = 1000)]
    [Alias("gtell")]
    [Alias("gsay")]
    [Syntax("[cmd] <message>")]
    public class GroupSay : PlayableCharacterGameAction
    {
        public string What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Say your group what?";

            What = CommandHelpers.JoinParameters(actionInput.Parameters);
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Act(ActOptions.ToGroup, "%g%{0:n} says the group '%x%{1}%g%'%x%", Actor, What);
            Actor.Send($"%g%You say to the group: '%x%{What}%g%'%x%");
        }
    }
}
