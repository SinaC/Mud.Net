using System;
using System.Reflection;
using Mud.DataStructures.Trie;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public class Admin : Player.Player, IAdmin
    {
        private static readonly Trie<MethodInfo> AdminCommands;

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

        public override IReadOnlyTrie<MethodInfo> Commands
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
        protected virtual bool DoIncarnate(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        // TODO: cause a crash in CommandHelpers.GetCommands
        //[Command("who")]
        //protected override bool DoWho(string rawParameters, CommandParameter[] parameters)
        //{
        //    Send("Admin who");
        //    return true;
        //}
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
