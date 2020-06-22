using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Communication
{
    [PlayerCommand("reply", "Communication")]
    [Syntax("[cmd] <message>")]
    public class Reply : TellGameActionBase
    {
        public string What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Reply what?";
            if (Actor.LastTeller == null)
                return StringHelpers.CharacterNotFound;

            What = actionInput.RawParameters;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            InnerTell(Actor.LastTeller, What);
        }
    }
}
