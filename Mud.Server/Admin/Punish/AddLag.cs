using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Admin.Punish
{
    [AdminCommand("addlag", "Punish")]
    [Syntax("[cmd] <player name> <tick>")]
    public class AddLag : AdminGameAction
    {
        private IPlayerManager PlayerManager { get; }
        private IWiznet Wiznet { get; }

        public IPlayer Whom { get; protected set; }
        public int Modifier { get; protected set; }

        public AddLag(IPlayerManager playerManager, IWiznet wiznet)
        {
            PlayerManager = playerManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length < 2)
                return BuildCommandSyntax();

            if (!actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();

            Modifier = actionInput.Parameters[1].AsNumber;
            if (Modifier <= 0)
                return "That makes a LOT of sense.";
            if (Modifier > 100)
                return "There's a limit to cruel and unusual punishment.";

            Whom = PlayerManager.GetPlayer(actionInput.Parameters[0], true);
            if (Whom == null)
                return StringHelpers.CharacterNotFound;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Wiznet.Wiznet($"{Actor.DisplayName} adds lag {Whom.DisplayName}.", Domain.WiznetFlags.Punish);

            Actor.Send("Adding lag now.");
            Whom.SetGlobalCooldown(Modifier);
        }
    }
}
