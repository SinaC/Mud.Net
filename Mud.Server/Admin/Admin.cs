using System;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public class Admin : Player.Player, IAdmin
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> AdminCommands;

        static Admin()
        {
            AdminCommands = CommandHelpers.GetCommands(typeof(Admin));
        }

        public Admin(IClient client, Guid id, string name) 
            : base(client, id, name)
        {
            //TODO: _currentStateMachine = new TestAdminStateMachine();
        }

        #region IAdmin

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return AdminCommands; }
        }

        #endregion

        public override void OnDisconnected()
        {
            base.OnDisconnected();

            // Stop incarnation if any
            if (Incarnating != null)
            {
                Incarnating.ChangeIncarnation(null);
                Incarnating = null;
            }
        }

        #endregion

        public IEntity Incarnating { get; private set; }

        #endregion

        [Command("incarnate")]
        protected virtual bool DoIncarnate(string rawParameters, params CommandParameter[] parameters)
        {
            return true;
        }

        // TODO: cause a crash in CommandHelpers.GetCommands
        [Command("who")]
        protected override bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Players:");
            foreach (IPlayer player in World.Instance.GetPlayers())
            {
                StringBuilder sb = new StringBuilder();
                switch (player.PlayerState)
                {
                    case PlayerStates.Connecting:
                    case PlayerStates.Connected:
                    case PlayerStates.CreatingAvatar:
                        sb.AppendFormat("[OOG] {0} {1}", player.DisplayName, player.PlayerState);
                        break;
                    case PlayerStates.Playing:
                        if (player.Impersonating != null)
                            sb.AppendFormat("[ IG] {0} playing {1}", player.DisplayName, player.Impersonating.Name);
                        else
                            sb.AppendFormat("[ IG] {0} playing ???", player.DisplayName);
                        break;
                }
                Send(sb.ToString());
            }
            Send("Admins");
            foreach (IAdmin admin in World.Instance.GetAdmins())
            {
                StringBuilder sb = new StringBuilder();
                switch (admin.PlayerState)
                {
                    case PlayerStates.Connecting:
                    case PlayerStates.Connected:
                    case PlayerStates.CreatingAvatar:
                        sb.AppendFormat("[OOG] {0} {1}", admin.DisplayName, admin.PlayerState);
                        break;
                    case PlayerStates.Playing:
                        if (admin.Impersonating != null)
                            sb.AppendFormat("[ IG] {0} impersonating {1}", admin.DisplayName, admin.Impersonating.Name);
                        else if (admin.Incarnating != null)
                            sb.AppendFormat("[ IG] {0} incarnating {1}", admin.DisplayName, admin.Incarnating.Name);
                        else
                            sb.AppendFormat("[ IG] {0} nor playing nor incarnating !!!", admin.DisplayName);
                        break;
                }
                Send(sb.ToString());
            }
            return true;
        }
    }

    // TODO: remove
    internal class TestAdminStateMachine : IInputTrap<IAdmin>
    {
        public bool IsFinalStateReached
        {
            get { return false; }
            
        }
        
        public void ProcessInput(IAdmin actor, string input)
        {
            // NOP
        }
    }
}
