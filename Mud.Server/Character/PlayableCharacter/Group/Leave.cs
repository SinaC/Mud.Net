using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Group
{
    [PlayableCharacterCommand("leave", "Group")]
    [Syntax("[cmd] <member>")]
    public class Leave : PlayableCharacterGameAction
    {
        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.Group == null)
                return "You aren't in a group.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Actor.Group.Members.Count() <= 2) // group will contain only one member, disband
                Actor.Group.Disband();
            else
                Actor.Group.RemoveMember(Actor);

        }
    }
}
