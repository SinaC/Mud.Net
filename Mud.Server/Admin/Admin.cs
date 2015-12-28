using System;
using System.Reflection;
using Mud.DataStructures;
using Mud.Network;

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
        }

        #region IActor

        public override IReadOnlyTrie<MethodInfo> Commands
        {
            get { return AdminCommands; }
        }

        #endregion

        #region IPlayer

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

        #region IAdmin

        public IEntity Incarnating { get; private set; }

        #endregion

        [Command("incarnate")]
        protected virtual bool Incarnate(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }
    }
}
