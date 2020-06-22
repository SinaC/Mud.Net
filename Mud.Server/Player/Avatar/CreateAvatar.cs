using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Settings;
using System.Linq;

namespace Mud.Server.Player.Avatar
{
    [PlayerCommand("createavatar", "Avatar", CannotBeImpersonated = true)]
    public class CreateAvatar : PlayerGameAction
    {
        private ISettings Settings { get; }

        public CreateAvatar(ISettings settings)
        {
            Settings = settings;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.Avatars.Count() >= Settings.MaxAvatarCount)
                return "Max. avatar count reached. Delete one before creating a new one.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Send("Please choose an avatar name (type quit to stop and cancel creation).");
            Actor.SetStateMachine(new AvatarCreationStateMachine());
        }
    }
}
