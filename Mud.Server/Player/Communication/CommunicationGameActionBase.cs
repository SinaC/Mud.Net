using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System;
using System.Linq;

namespace Mud.Server.Player.Communication
{
    public abstract class CommunicationGameActionBase : PlayerGameAction
    {
        private IPlayerManager PlayerManager { get; }

        protected abstract string NoParamMessage { get; }
        protected abstract string ActorSendPattern { get; }
        protected abstract string OtherSendPattern { get; }

        public string What { get; protected set; }

        protected CommunicationGameActionBase(IPlayerManager playerManager)
        {
            PlayerManager = playerManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return NoParamMessage;

            What = actionInput.RawParameters;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Send(ActorSendPattern, What);

            string other = String.Format(OtherSendPattern, Actor.DisplayName, What);
            foreach (IPlayer player in PlayerManager.Players.Where(x => x != Actor))
                player.Send(other);
        }
    }
}
