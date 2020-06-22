using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.PlayableCharacter.Shop
{
    [PlayableCharacterCommand("appraise", "Shop", "Identify")]
    [Syntax("[cmd] <item>")]
    public class Appraise : ShopPlayableCharacterGameActionBase
    {
        public Appraise(ITimeManager timeManager)
            : base(timeManager)
        {
        }

        public override void Execute(IActionInput actionInput)
        {
            // TODO: identify
            Actor.Send("Not Yet Implemented");
        }
    }
}
